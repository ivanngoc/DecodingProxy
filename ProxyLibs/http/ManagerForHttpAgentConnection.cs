using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Sniffing.Http;
using System.Collections.Concurrent;

namespace IziHardGames.Proxy
{
    public class ManagerForHttpAgentConnection
    {
        public ConcurrentDictionary<string, HttpAgentConnection> items = new ConcurrentDictionary<string, HttpAgentConnection>();

        public HttpAgentConnection Rent(string host)
        {
            var rent = PoolObjects<HttpAgentConnection>.Shared.Rent();
            var result = items.GetOrAdd(host, rent);
            if (rent != result)
            {
                PoolObjects<HttpAgentConnection>.Shared.Return(rent);
            }
            result.Host = host;
            return result;
        }
        public void Return(HttpAgentConnection rent)
        {
            HttpAgentConnection removed = default;
            while (!items.TryRemove(rent.Host, out removed))
            {

            }
            if (removed != rent)
            {
                removed.Dispose();
                PoolObjects<HttpAgentConnection>.Shared.Return(removed);
            }
            rent.Dispose();
            PoolObjects<HttpAgentConnection>.Shared.Return(rent);
        }
    }
}