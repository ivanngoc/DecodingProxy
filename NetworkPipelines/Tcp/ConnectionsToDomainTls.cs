using IziHardGames.Tls;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Libs.NonEngine.Memory;
using HttpDecodingProxy.ForHttp;
using System.Net.Sockets;
using System.Collections.Concurrent;
using IziHardGames.Proxy.Sniffing.ForHttp;
using IziHardGames.Libs.Networking.Pipelines;

namespace IziHardGames.Proxy.Tcp.Tls
{
    /// <summary>
    /// <see cref="ConnectionToDomain"/>
    /// </summary>
    public class ConnectionsToDomainTls : ConnectionsToDomain<TcpClientPipedSsl>
    {
        public X509Certificate2 ForgedCert => forgedCert;

        private X509Certificate2 forgedCert;
        private X509Certificate2 caCert;
        private X509Certificate2 originCert;
        private readonly ConcurrentDictionary<int, ConnectionToOriginTlsV1> connections = new ConcurrentDictionary<int, ConnectionToOriginTlsV1>();
        private StartOptions startOptions;

        public bool isSslProbed;
        private IPoolReturn<ConnectionsToDomainTls> pool;
        private Action<ConnectionsToDomainTls> returnToPool;

        public int Count => connections.Count;

        public void InitCerts(StartOptions options, X509Certificate2 ca)
        {
            caCert = ca;
            startOptions = options;
            Key = options.Host;

            if (CertManager.Shared.OriginalTryGetCertFromCacheWithWildcardSearching(options.HostAddress, out originCert))
            {
                isSslProbed = true;
                forgedCert = CertManager.Shared.ForgedGetOrCreateCertFromCache(originCert, caCert);
            }
        }

        public async Task<ConnectionToOriginTlsV1> CreateConnectionSsl(StartOptions options)
        {
            if (options.IsConnectRequired)
            {
                ConnectionToOriginTlsV1 connection;

                connection = PoolObjectsConcurent<ConnectionToOriginTlsV1>.Shared.Rent();

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

        public void Disconnect(ConnectionToOriginTlsV1 cto)
        {
            connections.TryRemove(new KeyValuePair<int, ConnectionToOriginTlsV1>(cto.id, cto));
        }

        public void UpdateCert(X509Certificate2 cert)
        {
            if (forgedCert == null || originCert == null)
            {
                this.originCert = cert;
                forgedCert = CertManager.Shared.ForgedGetOrCreateCertFromCache(cert, caCert);
            }
        }

        public void Dispose()
        {
            pool.Return(this);
            pool = default;
        }

        public void BindToPool(IPoolReturn<ConnectionsToDomainTls> pool)
        {
            this.pool = pool;
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void RegistRuturnToManager(Action<ConnectionsToDomainTls> returnToPool)
        {
            this.returnToPool = returnToPool;
        }

        public override async ValueTask<TcpClientPipedSsl> GetOrCreate(string title, IPoolObjects<TcpClientPipedSsl> pool)
        {
            var result = await base.GetOrCreate(title, pool);
            throw new System.NotImplementedException();
        }
    }
}