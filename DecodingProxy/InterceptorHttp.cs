using HttpDecodingProxy.http;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.TcpDecoder;
using System.Net;

namespace IziHardGames.Proxy.Sniffing.Http
{
    /// <summary>
    /// Both for HTTP/HTTPS
    /// </summary>
    public class InterceptorHttp
    {
        public bool isHttps;
        private TcpDecodingClient tcpDecodingClient;
        private HttpClientForIntercepting client;
        private HttpClientForInterceptingSsl clientSsl;
        private ManagerForHttpClientForIntercepting managerForHttpClientForIntercepting;
        private ManagerForHttpMessages manageForHttpMessages;
        private ManagerForHttpAgentConnection ManagerForConnections => decodingProxyServer.managerForConnections;
        private DecodingProxyServer decodingProxyServer;

        public InterceptorHttp(TcpDecodingClient tcpDecodingClient, ManagerForHttpClientForIntercepting managerForHttpClientForIntercepting, ManagerForHttpMessages manageForHttpMessages, DecodingProxyServer dps)
        {
            this.tcpDecodingClient = tcpDecodingClient;
            this.managerForHttpClientForIntercepting = managerForHttpClientForIntercepting;
            this.manageForHttpMessages = manageForHttpMessages;
            this.decodingProxyServer = dps;
        }


        public void Start()
        {
            var client = tcpDecodingClient.Client;
            var stream = client.GetStream();
            var initMsg = manageForHttpMessages.Initiate(stream);
            string hostFull = initMsg.request.fields.Host;
            string address = initMsg.request.fields.HostAddress;
            int port = initMsg.request.fields.HostPort;

            if (initMsg.request.fields.IsMethod(WebRequestMethods.Http.Connect))
            {
                if (managerForHttpClientForIntercepting.TryFindExisted(hostFull, out HttpClientForInterceptingSsl existed))
                {
                    Logger.LogLine($"HttpClient existed for {hostFull}");
                    existed.Terminate();
                    managerForHttpClientForIntercepting.Remove(hostFull);
                }
                initMsg.Dispose();

                clientSsl = managerForHttpClientForIntercepting.GetOrCreateV1(hostFull);
                stream.Write(HttpProxyMessage.response200);

                clientSsl.managerForMessages = manageForHttpMessages;
                clientSsl.managerForHttpAgentConnection = ManagerForConnections;

                clientSsl.StartTunnelModeSsl(client, decodingProxyServer.CaRootCert, address, port, hostFull);
                managerForHttpClientForIntercepting.Remove(hostFull);
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }
    }
}