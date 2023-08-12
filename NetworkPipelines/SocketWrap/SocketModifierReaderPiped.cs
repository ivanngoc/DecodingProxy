using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketModifierReaderPiped : SocketWrapModifier, IPoolBind<SocketModifierReaderPiped>
    {
        protected SocketReaderPiped? reader;
        protected IPoolReturn<SocketModifierReaderPiped>? pool;

        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            var pool = PoolObjectsConcurent<SocketReaderPiped>.Shared;
            SocketReaderPiped reader = pool.Rent();
            reader.BindToPool(pool);
            wrap.SetReader(reader);
            reader.Initilize(wrap.Socket);
        }

        public void BindToPool(IPoolReturn<SocketModifierReaderPiped> pool)
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