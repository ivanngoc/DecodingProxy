using IziHardGames.Libs.NonEngine.Memory;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy.Tcp.Tls
{
    public class ConnectionToOriginTls : IDisposable
    {
        public Stream rawStream;
        public Stream Stream { get; set; }
        public bool Connected => tcpClient?.Connected ?? false;
        public ConnectionsToDomainTls Ctd => connectionsToDomain;

        public SslStream sslStream;

        private TcpClient tcpClient;

        private ConnectionsToDomainTls connectionsToDomain;

        public readonly int id;

        public ConnectionToOriginTls()
        {
            id = GetHashCode();
        }

        public async Task ConnectSsl(ConnectionsToDomainTls connectionsToDomain, string address, int port)
        {
            this.connectionsToDomain = connectionsToDomain;
            Logger.LogLine($"Begin SSL connect to {address}:{port}:Count:{Ctd.Count}");
            await Connect(address, port).ConfigureAwait(false);
            var ssl = sslStream = new SslStream(rawStream);
            Stream = ssl;
            await ssl.AuthenticateAsClientAsync(address).ConfigureAwait(false);
            Ctd.UpdateCert((X509Certificate2)ssl.RemoteCertificate);
            Logger.LogLine($"SSL connection to {address}:{port} Established. Count:{Ctd.Count}");
        }
        public async Task Connect(string host, int port)
        {
            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port).ConfigureAwait(false);
            this.tcpClient = tcpClient;
            var ns = tcpClient.GetStream();
            rawStream = ns;
            Stream = rawStream;

            ns.ReadTimeout = 60000;
        }

        public void Dispose()
        {
            rawStream = default;
            sslStream = default;
            tcpClient = default;

            PoolObjectsConcurent<ConnectionToOriginTls>.Shared.Return(this);
        }
    }
}