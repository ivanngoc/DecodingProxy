using IziHardGames.Core;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Tls;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.Tcp.Tls
{
    /// <summary>
    /// <see cref="ConnectionToDomain"/>
    /// </summary>
    public class ConnectionsToDomainSsl<T> : ConnectionsToDomain<T> where T : ISslEndpoint, IInitializable<string>, IPoolBind<T>, IClient, IConnectorTcp, IKey<uint>, IConnection, IConnectionData, IDisposable
    {
        public X509Certificate2 ForgedCert => forgedCert;

        private X509Certificate2 forgedCert;
        private X509Certificate2 caCert;
        private X509Certificate2 originCert;

        public bool isSslProbed;
        private IPoolReturn<ConnectionsToDomainSsl<T>> pool;
        private new Action<ConnectionsToDomainSsl<T>> returnToManager;


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

        public void BindToPool(IPoolReturn<ConnectionsToDomainSsl<T>> pool)
        {
            this.pool = pool;
        }

        public void RegistRuturnToManager(Action<ConnectionsToDomainSsl<T>> returnToPool)
        {
            this.returnToManager = returnToPool;
        }

        protected async override ValueTask<T> CreateAsync(string title, IPoolObjects<T> pool)
        {
            var client = await base.CreateAsync(title, pool).ConfigureAwait(false);
            await client.AuthAsClientAsync().ConfigureAwait(false);
            monitor.OnUpdate(client);
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