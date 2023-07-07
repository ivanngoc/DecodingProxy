using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using HttpDecodingProxy.ForHttp;

namespace IziHardGames.Proxy.Http
{

    /// <summary>
    /// CONNECT is intended for use in requests to a proxy. 
    /// The recipient can establish a tunnel either by directly connecting to the server identified by the request target or, 
    /// if configured to use another proxy, by forwarding the CONNECT request to the next inbound proxy. 
    /// An origin server MAY accept a CONNECT request, but most origin servers do not implement CONNECT.
    /// </summary>
    public class ConnectionHttp : IConnection, IDisposable
    {
        private HttpFieldsV11 headers;
        private TcpClient client;
        private NetworkStream clientStream;
        private SslStream sslClient;
        private SslStream sslOrigin;
        private readonly string certPwd = "1234";
        private readonly string certName = "ngoc_ssl.cer";
        private readonly string certNamePvk = "ngoc_ssl.pvk";
        private readonly string certPath = "/cert/";
        public object ConnectToProx { get; }

        public ConnectionHttp(HttpFieldsV11 headers, System.Net.Sockets.TcpClient client)
        {
            this.headers = headers;
            this.client = client;
            this.clientStream = client.GetStream();
        }

        //  https://httpwg.org/specs/rfc9110.html#CONNECT
        //TODO: A server MUST reject a CONNECT request that targets an empty or invalid port number, typically by responding with a 400 (Bad Request) status code.

        public void Connect()
        {
            TcpClient origin = new TcpClient();
            origin.Connect(headers.HostAddress, headers.HostPort);

            if (false)
            {
                Tunneling(client, origin);
                TunnelingSsl();
            }
        }
        public void ConnectToProxy()
        {
            throw new NotImplementedException();
        }
        public static void Tunneling(TcpClient client, TcpClient origin)
        {
            Task.Run(() => TransferFromTo(client, origin));
            Task.Run(() => TransferFromTo(origin, client));
        }

        private void FromOriginToClient()
        {
            var handler = new HttpClientHandler();
            HttpClient httpClient = new HttpClient(handler);
            //httpClient.Send();
            WebClient webClient = new WebClient();
        }

        private static void TransferFromTo(TcpClient from, TcpClient to)
        {
            byte[] buffer = new byte[1024];
            int readed = from.GetStream().Read(buffer);

            if (readed > 0)
            {
                to.GetStream().Write(buffer, 0, readed);
            }
        }

        public void TunnelingSsl()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), certPath, certName);
            SslStream clientSsl = new SslStream(clientStream, false);
            var certificate = new X509Certificate2(@"C:\Users\ngoc\Documents\[Projects] C#\DecodingProxy\DecodingProxy\cert\ProxyMitmServer.cer", certPwd);
            clientSsl.AuthenticateAsServer(certificate, false, SslProtocols.Default, false);

            TcpClient server = new TcpClient(headers.HostAddress, headers.HostPort);
            SslStream serverSslStream = new SslStream(server.GetStream(), false, SslValidationCallback, null);
            serverSslStream.AuthenticateAsClient(headers.HostAddress);

            this.sslClient = clientSsl;
            this.sslOrigin = serverSslStream;
        }

        private bool SslValidationCallback(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static ConnectionHttp Test(TcpClient client)
        {
            ConnectionHttp http = new ConnectionHttp(new HttpFieldsV11(), client);
            http.TunnelingSsl();
            throw new NotImplementedException();
        }

        public static void Test1(TcpClient client)
        {
            HttpListener httpListener = new HttpListener();
            throw new System.NotImplementedException();
        }


    }
}