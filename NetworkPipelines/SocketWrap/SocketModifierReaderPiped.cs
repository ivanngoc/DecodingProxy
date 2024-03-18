using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketModifierReaderPiped : SocketWrapModifier, IPoolBind<SocketModifierReaderPiped>
    {
        protected SocketReaderPiped? reader;
        protected IPoolReturn<SocketModifierReaderPiped>? pool;
        public IReader Reader => reader!;
        private Task? taskWriterLoop;
        private CancellationTokenSource? ctsWriter;

        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            var pool = PoolObjectsConcurent<SocketReaderPiped>.Shared;
            SocketReaderPiped reader = pool.Rent();
            reader.BindToPool(pool);
            this.reader = reader;
            reader.Initilize(wrap);
        }

        public override void InitilizeReverse()
        {
            base.InitilizeReverse();
            reader!.Dispose();
            reader = default;
            if (taskWriterLoop!.Status != TaskStatus.RanToCompletion) ctsWriter!.Cancel();
            ctsWriter = default;
            taskWriterLoop = default;
        }

        public void BindToPool(IPoolReturn<SocketModifierReaderPiped> pool)
        {
            this.pool = pool;
        }
        public async Task RunWriter(CancellationToken ct = default)
        {
            ctsWriter = new CancellationTokenSource();
            taskWriterLoop = reader!.RunWriter(ctsWriter.Token);
            await taskWriterLoop.ConfigureAwait(false);
        }
        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;
        }
    }
}