using HttpDecodingProxy.http;
using IziHardGames.Tls;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy.Sniffing.Http
{
    /// <summary>
    /// Connection beetween <see cref="DecodingProxyServer"/> and accepted tcp client from agent
    /// </summary>
    public class HttpAgentConnection
    {
        public string Host { get; set; }
        public NetworkStream StreamRaw => rawStream;
        public Stream Stream => stream;
        private TcpClient tcpClient;
        private Stream stream;
        private NetworkStream rawStream;
        private SslStream streamSslToAgent;

        private HttpClientForIntercepting httpClient;
        private ManagerForHttpMessages managerForMessages;
        private X509Certificate2 caCert;
        private bool isAwaitResponse;

        internal void Dispose()
        {
            Host = string.Empty;
            tcpClient = default;
            stream = default;
            rawStream = default;
            streamSslToAgent = default;
            httpClient = default;
            managerForMessages = default;
            caCert = default;
        }

        public void Initilize(TcpClient tcpClient, HttpClientForIntercepting httpClient, ManagerForHttpMessages managerForMessages, X509Certificate2 caCert)
        {
            this.httpClient = httpClient;
            this.tcpClient = tcpClient;
            this.managerForMessages = managerForMessages;
            this.caCert = caCert;
            this.rawStream = tcpClient.GetStream();
        }

        public void Run()
        {
            while (tcpClient.Connected)
            {
                lock (this)
                {
                    if (isAwaitResponse) continue;
                }

                var msg = managerForMessages.GetNew();
                msg.Bind(this);

                try
                {
                    msg.ReadMsgInto(stream, rawStream, msg.request);
                    SetAwaitResponse(true);
                    Logger.LogLine($"Incoming MSG: {msg.request.fields.ToStringInfo()}", ConsoleColor.Magenta);
                }
                catch (IOException ex)
                {
                    Logger.LogException(ex);
                    return;
                }
                httpClient.EnqueueMsg(msg);
            }
        }

        public void ForgeAgentSsl(X509Certificate2 serverCert)
        {
            Logger.LogLine($"Begin forge server cert: {serverCert.Subject}");
            var cert = CertManager.GenerateCertEndpoint(serverCert, caCert, DateTime.Now.AddYears(5));
            var ssl = new SslStream(rawStream);
            ssl.AuthenticateAsServer(cert);
            //ssl.AuthenticateAsServer(cert, false, streamSslToOrigin.SslProtocol, false);
            streamSslToAgent = ssl;
            this.stream = ssl;
            Logger.LogLine($"Forge server cert: {serverCert.Subject} complete");
        }

        public void FinishConectMethod()
        {
            rawStream.Write(HttpProxyMessage.response200);
        }

        public void SetAwaitResponse(bool isAwaitResponse)
        {
            lock (this)
            {
                this.isAwaitResponse = isAwaitResponse;
            }
        }
    }
}