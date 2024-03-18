using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using IziHardGames.Pools.Abstractions.NetStd21;
using System.Runtime.CompilerServices;

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
            rent.Initilize(wrap);
            if (rent is IPoolBind<SocketWriterDefault> poolable) poolable.BindToPool(pool);
            wrap.SetWriter(rent);
        }
    }

    public class SocketWriterDefault : SocketWriter, IPoolBind<SocketWriterDefault>
    {
        private IPoolReturn<SocketWriterDefault>? pool;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int Write(byte[] array, int offset, int length)
        {
            return source!.Write(array, offset, length);
        }
        public override async Task WriteAsync(byte[] array, CancellationToken token = default)
        {
            await source!.WriteAsync(array, token).ConfigureAwait(false);
        }
        public override async Task WriteAsync(ReadOnlyMemory<byte> readOnlyMemory, CancellationToken token = default)
        {
            await source!.WriteAsync(readOnlyMemory, token).ConfigureAwait(false);
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