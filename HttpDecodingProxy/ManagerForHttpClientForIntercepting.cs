using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Http;
using IziHardGames.Proxy.Sniffing.ForHttp;
using IziHardGames.Proxy.Tcp;
using IziHardGames.Proxy.Tcp.Tls;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Dic = System.Collections.Concurrent.ConcurrentDictionary<int, IziHardGames.Proxy.Sniffing.ForHttp.ProxyBridge>;

namespace IziHardGames.Proxy
{

    public class ManagerForHttpClientForIntercepting
    {
        /// <summary>
        /// Key = HostAddress + HostPort
        /// </summary>
        public Dic items = new Dic();
        private ManagerForConnectionsToDomain mctd;
        private ManagerForConnectionToAgent mcta;
        private ManagerForHttpMessages managerForHttpMessages;

        public void Init(ManagerForConnectionsToDomain managerForConnectionsToDomain, ManagerForConnectionToAgent managerForConnectionToAgent, ManagerForHttpMessages managerForHttpMessages)
        {
            this.mctd = managerForConnectionsToDomain;
            this.mcta = managerForConnectionToAgent;
            this.managerForHttpMessages = managerForHttpMessages;
        }

        public async Task<ProxyBridge> CreateBridge(HttpSpyProxy server, ConnectionsToDomainTls ctd, TcpWrap tcpDecodingClient, StartOptions options)
        {
            string host = options.Host;
            ProxyBridge client;
            ConnectionToOriginTlsV1 cto;

            if (options.IsHttps)
            {
                var connection = await ctd.CreateConnectionSsl(options).ConfigureAwait(false);
                var clientHttps = PoolObjects<ProxyBridgeSsl>.Shared.Rent();
                if (!items.TryAdd(clientHttps.id, clientHttps))
                {
                    throw new Exception($"Can't add connection");
                }
                client = clientHttps;
                cto = connection;
            }
            else
            {
                throw new System.NotSupportedException();
            }
            client.InitToReuse(ctd, options);

            ConnectionToAgentTls cta = mcta.Rent();
            cta.InitToReuse(host, tcpDecodingClient.Client, client, managerForHttpMessages, ctd.ForgedCert);
            await client.MakeInterceptingBridge(cto, cta).ConfigureAwait(false);
            return client;
        }

        public void Remove(ProxyBridge item)
        {
            if (!items.TryRemove(new KeyValuePair<int, ProxyBridge>(item.id, item)))
            {
                throw new Exception($"Can't delete");
            }
        }
    }
}