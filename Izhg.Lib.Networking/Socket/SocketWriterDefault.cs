using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public class SocketModifierWriterDefault : SocketWrapModifier, IPoolBind<SocketModifierWriterDefault>
    {
        private IPoolReturn<SocketModifierWriterDefault>? pool;
        protected SocketWriterDefault? writer;

        public void BindToPool(IPoolReturn<SocketModifierWriterDefault> pool)
        {
            this.pool = pool;
        }
        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;
        }
        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            var pool = PoolObjectsConcurent<SocketWriterDefault>.Shared;
            var rent = writer = pool.Rent();
            rent.Initilize(wrap.Socket);
            if (rent is IPoolBind<SocketWriterDefault> poolable) poolable.BindToPool(pool);
            wrap.SetWriter(rent);
        }
    }

    public class SocketWriterDefault : SocketWriter, IPoolBind<SocketWriterDefault>
    {
        private IPoolReturn<SocketWriterDefault>? pool;
        public override async Task SendAsync(byte[] array, CancellationToken token = default)
        {
            await socket!.SendAsync(array, SocketFlags.None, token).ConfigureAwait(false);
        }
        public override async Task SendAsync(ReadOnlyMemory<byte> readOnlyMemory, CancellationToken token = default)
        {
            await socket!.SendAsync(readOnlyMemory, SocketFlags.None, token).ConfigureAwait(false);
        }
        internal static SocketWriter Rent()
        {
            throw new NotImplementedException();
        }

        public void BindToPool(IPoolReturn<SocketWriterDefault> pool)
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