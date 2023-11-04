using IziHardGames.Core;
using IziHardGames.Libs.Cryptography.Defaults;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Tls;
using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.Tcp.Tls
{
    /// <summary>
    /// <see cref="ConnectionToDomain"/>
    /// </summary>
    public class ConnectionsToDomainTls<T> : ConnectionsToDomain<T>
        where T : IInitializable<string>, IPoolBind<T>, IClient<SocketReader, SocketWriter>, IConnectorTcp, IKey<uint>, IConnection<ConnectionDataPoolable>, IGetConnectionData<ConnectionDataPoolable>, IDisposable, IUpgradableConnection<SocketWrapUpgrade>
    {
        public X509Certificate2 ForgedCert => forgedCert;

        private X509Certificate2 forgedCert;
        private X509Certificate2 caCert;
        private X509Certificate2 originCert;

        public bool isSslProbed;
        private IPoolReturn<ConnectionsToDomainTls<T>> pool;
        private new Action<ConnectionsToDomainTls<T>> returnToManager;


        public async Task UpdateCertAsync(X509Certificate2 cert)
        {
            if (forgedCert == null || originCert == null)
            {
                this.originCert = cert;
                forgedCert = await CertManager.Shared.ForgedGetOrCreateCertFromCacheAsync(cert, caCert).ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            pool.Return(this);
            pool = default;
        }

        public void BindToPool(IPoolReturn<ConnectionsToDomainTls<T>> pool)
        {
            this.pool = pool;
        }

        public void RegistRuturnToManager(Action<ConnectionsToDomainTls<T>> returnToPool)
        {
            this.returnToManager = returnToPool;
        }

        protected async override ValueTask<T> CreateAsync(EConnectionFlags filter, string title, IPoolObjects<T> pool)
        {
            var client = await base.CreateAsync(filter, title, pool).ConfigureAwait(false);
            SocketWrapUpgradeTlsHttp upgrade;
            if (filter.HasFlag(EConnectionFlags.HTTP2) && filter.HasFlag(EConnectionFlags.TLS12))
            {
                upgrade = client.UpgradeTls(SslDefaults.DefaultHttp2) as SocketWrapUpgradeTlsHttp ?? throw new NullReferenceException();
                goto NEXT;
            }
            else if (filter.HasFlag(EConnectionFlags.HTTP11) && filter.HasFlag(EConnectionFlags.TLS12))
            {
                upgrade = client.UpgradeTls(null) as SocketWrapUpgradeTlsHttp ?? throw new NullReferenceException();
                goto NEXT;
            }
            else throw new System.NotImplementedException();
            NEXT:
            ISslEndpoint sslEndpoint = upgrade as ISslEndpoint ?? throw new NullReferenceException();
            await sslEndpoint.AuthAsClientAsync().ConfigureAwait(false);
            monitor.OnUpdate(client.ConnectionData);
            return client;
        }

        public bool TryGetForgedCert(out X509Certificate2 forgedOrigin)
        {
            forgedOrigin = forgedCert;
            return forgedOrigin != null;
        }

        public void SetForgedCert(X509Certificate2 forgedCert)
        {
            this.forgedCert = forgedCert;
        }
        protected override void ReturnToManager()
        {
            returnToManager(this);
        }
    }
}