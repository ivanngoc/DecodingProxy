using IziHardGames.Tls;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Libs.NonEngine.Memory;
using HttpDecodingProxy.ForHttp;
using System.Net.Sockets;
using System.Collections.Concurrent;
using IziHardGames.Proxy.Sniffing.ForHttp;

namespace IziHardGames.Proxy.Tcp.Tls
{
    public class ConnectionsToDomainTls
    {
        public string AddressAndPort { get; set; }
        public X509Certificate2 ForgedCert => forgedCert;

        private X509Certificate2 forgedCert;
        private X509Certificate2 caCert;
        private X509Certificate2 originCert;

        private readonly ConcurrentDictionary<int, ConnectionToOriginTls> connections = new ConcurrentDictionary<int, ConnectionToOriginTls>();
        private StartOptions startOptions;

        public bool isSslProbed;
        public int Count => connections.Count;

        public void InitAsync(StartOptions options, X509Certificate2 ca)
        {
            caCert = ca;
            startOptions = options;
            AddressAndPort = options.Host;

            if (CertManager.Shared.OriginalTryGetCertFromCacheWithWildcardSearching(options.HostAddress, out originCert))
            {
                isSslProbed = true;
                forgedCert = CertManager.Shared.ForgedGetOrCreateCertFromCache(originCert, caCert);
            }
        }

        public async Task<ConnectionToOriginTls> CreateConnectionSsl(StartOptions options)
        {
            if (options.IsConnectRequired)
            {
                ConnectionToOriginTls connection;

                connection = PoolObjectsConcurent<ConnectionToOriginTls>.Shared.Rent();

                if (!connections.TryAdd(connection.id, connection))
                {
                    throw new Exception($"Can't add to Concurent dictionary key:{connection.id}");
                }
                await connection.ConnectSsl(this, options.HostAddress, options.HostPort).ConfigureAwait(false);
                return connection;
            }
            throw new ArgumentException($"SSL Requred");
        }

        public async Task ProbeSsl(CancellationToken token)
        {
            lock (this)
            {
                if (isSslProbed) return;

                StartOptions options = startOptions;

                using (TcpClient tcpClient = new TcpClient())
                {
                    tcpClient.Connect(options.HostAddress, options.HostPort);

                    using (var ssl = new SslStream(tcpClient.GetStream()))
                    {
                        try
                        {
                            ssl.AuthenticateAsClient(options.HostAddress);
                            originCert = (X509Certificate2)ssl.RemoteCertificate;
                            CertManager.Shared.OriginalSaveToCacheWithMultipleDomains(originCert);
                            forgedCert = CertManager.Shared.ForgedGetOrCreateCertFromCache(originCert, caCert);
                            isSslProbed = true;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex);
                            options.cts.Cancel();
                        }
                    }
                }
            }
        }

        public void Disconnect(ProxyBridge httpClientForIntercepting)
        {
            connections.TryRemove(new KeyValuePair<int, ConnectionToOriginTls>(httpClientForIntercepting.Cto.id, httpClientForIntercepting.Cto));
        }

        public void UpdateCert(X509Certificate2 cert)
        {
            if (forgedCert == null || originCert == null)
            {
                this.originCert = cert;
                forgedCert = CertManager.Shared.ForgedGetOrCreateCertFromCache(cert, caCert);
            }
        }
    }
}