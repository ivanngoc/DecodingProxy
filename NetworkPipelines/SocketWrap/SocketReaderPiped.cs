using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.Pipelines;
using IziHardGames.Libs.Pipelines.Contracts;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketReaderPiped : SocketReader, IReader, IPoolBind<SocketReaderPiped>, IGetPipeReader
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
                var readed = await source!.TransferToAsync(memory, token).ConfigureAwait(false);
                writer.Advance(readed);
                if (readed > 0)
                {
                    FlushResult result = await writer.FlushAsync(token).ConfigureAwait(false);
                }
                else
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }
        }

        public async ValueTask<ReadResult> ReadPipeAsync(CancellationToken token = default)
        {
            return await reader.ReadAsync(token).ConfigureAwait(false);
        }
        public async override ValueTask<int> TransferToAsync(Memory<byte> mem, CancellationToken ct = default)
        {
            var result = await reader.ReadAsync().ConfigureAwait(false);
            var lengthBuffer = (int)result.Buffer.Length;
            int length = lengthBuffer > mem.Length ? mem.Length : lengthBuffer;
            var slice = result.Buffer.Slice(0, length);
            slice.CopyTo(mem.Span);
            reader.AdvanceTo(slice.End);
            return length;
        }

        public void ReportConsume(SequencePosition position)
        {
            reader.AdvanceTo(position);
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

        public override int TransferTo(byte[] array, int offset, int length)
        {
            if (reader.TryRead(out ReadResult result))
            {
                int readed = (int)result.Buffer.Length;
                int toCopy = readed > length ? length : readed;
                result.Buffer.CopyTo(new Span<byte>(array, offset, toCopy));
                return toCopy;
            }
            throw new System.NotImplementedException();
        }
    }
}