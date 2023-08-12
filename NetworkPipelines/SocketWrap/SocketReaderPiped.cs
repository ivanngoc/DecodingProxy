using System;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.Pipelines;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketReaderPiped : SocketReader, IReader, IPoolBind<SocketReaderPiped>, IPipeReader
    {
        protected IPoolReturn<SocketReaderPiped>? pool;
        protected readonly Pipe pipe;
        protected readonly PipeReader reader;
        protected readonly PipeWriter writer;
        public PipeReader Reader { get => reader; }
        public static PipeOptions OptionsDefault => SharedPipes.pipeOptions;

        public SocketReaderPiped() : base()
        {
            pipe = new Pipe(OptionsDefault);
            reader = pipe.Reader;
            writer = pipe.Writer;
        }

        public async Task RunWriter(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Memory<byte> memory = writer.GetMemory(1 << 20);
                var bytesRead = await socket!.ReceiveAsync(memory, SocketFlags.None, token).ConfigureAwait(false);
                writer.Advance(bytesRead);
                FlushResult result = await writer.FlushAsync().ConfigureAwait(false);
            }
        }

        public async ValueTask<ReadResult> ReadPipeAsync(CancellationToken token = default)
        {
            return await reader.ReadAsync(token).ConfigureAwait(false);
        }
        public void ReportConsume(SequencePosition position)
        {
            throw new NotImplementedException();
        }
        public void BindToPool(IPoolReturn<SocketReaderPiped> pool)
        {
            this.pool = pool;
        }
        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;
            reader.Complete();
            writer.Complete();
            pipe.Reset();
        }

        public void AdvanceTo(SequencePosition end)
        {
            reader.AdvanceTo(end);
        }
    }
}