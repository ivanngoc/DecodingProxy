using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp.Tls;
using System.Collections.Concurrent;

namespace IziHardGames.Proxy
{
    public class ManagerForConnectionToAgent
    {
        public ConcurrentBag<ConnectionToAgentTls> items = new ConcurrentBag<ConnectionToAgentTls>();

        public ConnectionToAgentTls Rent()
        {
            var rent = PoolObjects<ConnectionToAgentTls>.Shared.Rent();
            items.Add(rent);
            return rent;
        }
        public void Return(ConnectionToAgentTls rent)
        {
            while (!items.TryTake(out rent))
            {

            }
            rent.Dispose();
            PoolObjects<ConnectionToAgentTls>.Shared.Return(rent);
        }
    }
}