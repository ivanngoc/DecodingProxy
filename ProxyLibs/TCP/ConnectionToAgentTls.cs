using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Sniffing.ForHttp;
using IziHardGames.Tls;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy.Tcp.Tls
{
    /// <summary>
    /// Connection beetween <see cref="DecodingProxyServer"/> and accepted tcp client from agent
    /// </summary>
    public class ConnectionToAgentTls
    {
        public string Host { get; set; }
        public NetworkStream StreamRaw => rawStream;
        public SslStream SslStream => streamSslToAgent;
        public Stream Stream => stream;

        public bool Connected => tcpClient?.Connected ?? false;

        private TcpClient tcpClient;
        private Stream stream;
        private NetworkStream rawStream;
        private SslStream streamSslToAgent;

        private ProxyBridge httpClient;
        private ManagerForHttpMessages managerForMessages;
        private X509Certificate2 forgedOriginCert;
        private static int counter = 1000;
        private readonly int id;
        private static readonly List<ConnectionToAgentTls> items = new List<ConnectionToAgentTls>();

        public ConnectionToAgentTls()
        {
            id = counter++;
            items.Add(this);
        }

        internal void Dispose()
        {
            Host = string.Empty;
            tcpClient = default;
            stream = default;
            rawStream = default;
            streamSslToAgent = default;
            httpClient = default;
            managerForMessages = default;
            forgedOriginCert = default;

            PoolObjects<ConnectionToAgentTls>.Shared.Return(this);
        }

        public void InitToReuse(string host, TcpClient tcpClient, ProxyBridge httpClient, ManagerForHttpMessages managerForMessages, X509Certificate2 forgedCert)
        {
            Host = host;
            this.httpClient = httpClient;
            this.tcpClient = tcpClient;
            this.managerForMessages = managerForMessages;
            this.forgedOriginCert = forgedCert;
            rawStream = tcpClient.GetStream();
        }

        public void FinishConectMethod()
        {
            rawStream.Write(HttpProxyMessage.response200);
        }

        public async Task ForgeSslConnection()
        {
            var cert = forgedOriginCert;
            Logger.LogLine($"[{id}] Begin forge ssl connection To Agent: {cert.Subject}");
            var ssl = new SslStream(rawStream);
            await ssl.AuthenticateAsServerAsync(cert).ConfigureAwait(false);
            streamSslToAgent = ssl;
            stream = ssl;
            Logger.LogLine($"[{id}] Forge ssl connection: {cert.Subject} complete");
        }
    }
}