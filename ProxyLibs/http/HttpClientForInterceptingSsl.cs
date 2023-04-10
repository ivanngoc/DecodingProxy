using HttpDecodingProxy.http;
using IziHardGames.Libs.NonEngine.Memory;
using ProxyLibs.Extensions;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy.Sniffing.Http
{
    public class HttpClientForInterceptingSsl : HttpClientForIntercepting
    {
        private X509Certificate2 caCert;
        private X509Certificate2 serverCert;

        private SslStream streamSslToOrigin;
        private SslStream streamSslToAgent;

        public SslStream SslToOrigin => streamSslToOrigin;
        public SslStream SslToAgent => streamSslToAgent;

        public void Init(X509Certificate2 caRootCert)
        {
            caCert = caRootCert;
        }

        private void ConnectToOriginSsl(string address, int port)
        {
            Logger.LogLine($"Begin SSL connect to {address}:{port}");
            ConnectToOrigin(address, port);
            SslStream sslStream = new SslStream(tcpToOrigin.GetStream(), false, SslValidationCallback, null);
            sslStream.AuthenticateAsClient(address);
            streamSslToOrigin = sslStream;
            this.streamToOrigin = streamSslToOrigin;
            serverCert = (X509Certificate2)sslStream.RemoteCertificate;
            Logger.LogLine($"SSL connection to {address}:{port} Established");
        }

        private bool SslValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public override void Dispose()
        {
            base.Dispose();

            streamSslToOrigin = default;
            streamSslToAgent = default;


            PoolObjects<HttpClientForInterceptingSsl>.Shared.Return(this);
        }


        // Possible double connection to origin in the same time?
        protected override void MakeInterceptingBridge(HttpRequest request, HttpAgentConnection agent)
        {
            if (request.fields.IsConnectRequired)
            {
                ConnectToOriginSsl(request.fields.HostAddress, request.fields.HostPort);
                agent.FinishConectMethod();
                Logger.LogLine($"Finished CONNECT method to {agent.Host}");
                agent.ForgeAgentSsl(serverCert);
                IsConnecteToOrigin = tcpToOrigin.Connected;
                agents.Add(agent);
            }
            else
            {
                base.MakeInterceptingBridge(request, agent);
            }
        }

        public void StartTunnelModeSsl(TcpClient client, X509Certificate2 caRootCert, string address, int port, string addressAndPort)
        {
            Start();

            var agent = managerForHttpAgentConnection.Rent(addressAndPort);
            agent.Initilize(client, this, managerForMessages, caCert);
            var streamToAgent = agent.Stream;

            this.AddressAndPort = addressAndPort;
            this.caCert = caRootCert;

            ConnectToOriginSsl(address, port);
            agent.ForgeAgentSsl(serverCert);
            var nsToOrigin = tcpToOrigin.GetStream();

            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    var msg = managerForMessages.GetNew();

                    msg.ReadMsgInto(streamToAgent, agent.StreamRaw, msg.request);

                    Task.Run(() => OnActionRequest(msg.request.DeepCopy()));
                    // modify
                    ActionModifyRequest(msg.request);
                    // send
                    msg.request.Write(streamToOrigin);

                    msg.ReadMsgInto(streamToOrigin, nsToOrigin, msg.response);

                    Task.Run(() => OnActionResponse(msg.response.DeepCopy()));

                    ActionModifyResponse(msg.response);

                    msg.response.Write(streamToAgent);

                    msg.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    break;
                }
            }
            Dispose();
        }
    }
}