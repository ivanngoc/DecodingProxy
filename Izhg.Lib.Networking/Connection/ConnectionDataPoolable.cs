using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.Contracts
{
    public class ConnectionDataPoolable : ConnectionDataDefault, IPoolBind<ConnectionDataPoolable>
    {
        private IPoolReturn<ConnectionDataPoolable> pool;
        public void BindToPool(IPoolReturn<ConnectionDataPoolable> pool)
        {
            this.pool = pool;
        }
        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;
        }
    }
}