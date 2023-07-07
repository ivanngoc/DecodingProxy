using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Http;
using IziHardGames.Proxy.Tcp;
using IziHardGames.Proxy.Tcp.Tls;
using System.Net;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    /// <summary>
    /// Both for HTTP/HTTPS
    /// </summary>
    public class InterceptorHttp
    {
        public bool isHttps;
        private TcpWrap tcpDecodingClient;
        private ProxyBridge client;
        private ProxyBridgeSsl clientSsl;
        private ManagerForHttpClientForIntercepting managerForHttpClientForIntercepting;
        private ManagerForHttpMessages manageForHttpMessages;
        private ManagerForConnectionToAgent ManagerForConnections => decodingProxyServer.managerForConnectionsToAgent;
        private ManagerForConnectionsToDomain ManagerForConnectionsToDomain => decodingProxyServer.managerForConnectionsToDomain;
        private HttpSpyProxy decodingProxyServer;

        public InterceptorHttp(TcpWrap tcpDecodingClient, ManagerForHttpClientForIntercepting managerForHttpClientForIntercepting, ManagerForHttpMessages manageForHttpMessages, HttpSpyProxy dps)
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
            var initMsg = manageForHttpMessages.ReadFirstRequest(stream);
            var options = initMsg.ToStartOptions();
            string hostFull = initMsg.request.fields.Host;
            string address = initMsg.request.fields.HostAddress;
            int port = initMsg.request.fields.HostPort;
            ConnectionsToDomainTls ctd = ManagerForConnectionsToDomain.GetOrCreate(options, decodingProxyServer.CaRootCert);

            //if (initMsg.request.fields.IsMethod(WebRequestMethods.Http.Connect))
            //{
            //    HttpClientForInterceptingSsl existed = (HttpClientForInterceptingSsl)managerForHttpClientForIntercepting.Create(decodingProxyServer, ctd, tcpDecodingClient, options);

            //    initMsg.Dispose();

            //    clientSsl = managerForHttpClientForIntercepting.GetOrCreateV1(hostFull);
            //    stream.Write(HttpProxyMessage.response200);

            //    clientSsl.managerForMessages = manageForHttpMessages;

            //    //clientSsl.StartTunnelModeSsl(client, decodingProxyServer.CaRootCert, address, port, hostFull);
            //    managerForHttpClientForIntercepting.Remove(existed);
            //}
            //else
            //{
            //    throw new System.NotImplementedException();
            //}
        }
    }
}