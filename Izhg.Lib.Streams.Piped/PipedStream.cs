using IziHardGames.Pools.Abstractions.NetStd21;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Threading.Tasks;
using PipeOptions = System.IO.Pipelines.PipeOptions;

namespace IziHardGames.Libs.Streaming
{
    /// <summary>
    /// <see cref="MemStream"/>
    /// </summary>
    public class PipedStream : Stream, IGuid
    {
        public override bool CanRead { get => true; }
        public override bool CanSeek { get => throw new System.NotImplementedException(); }
        public override bool CanWrite { get => true; }
        public override long Length { get => throw new System.NotImplementedException(); }
        public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public readonly Pipe pipe;
        public readonly PipeWriter writer;
        public readonly PipeReader reader;

        public readonly Guid guid;
        public Guid Guid => guid;

        public virtual PipeReader Reader { get => reader; set => throw new NotImplementedException(); }

        public PipedStream(PipeOptions pipeOptions) : base()
        {
            guid = Guid.NewGuid();
            this.pipe = new Pipe(pipeOptions);
            writer = pipe.Writer;
            reader = pipe.Reader;
        }

        public PipedStream() : base()
        {
            guid = Guid.NewGuid();
            this.pipe = new Pipe(new PipeOptions(null, null, null, -1, -1, (1 << 20) * 8, false));
            writer = pipe.Writer;
            reader = pipe.Reader;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public async Task<int> Consume()
        {
            ReadResult result = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = result.Buffer;
            reader.AdvanceTo(buffer.Start, buffer.End);
            // do something
            await reader.CompleteAsync().ConfigureAwait(false);

            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(ReadOnlySpan<byte> span)
        {
            var toWriteInto = writer.GetSpan(span.Length);
            span.CopyTo(toWriteInto);
            writer.Advance(span.Length);
            throw new System.NotImplementedException();
        }

        public void WriteFrom(Stream stream)
        {
            var toWriteInto = writer.GetSpan();
            int readed = stream.Read(toWriteInto);
            writer.Advance(readed);
        }

        public async Task WriteFromAsync(Stream stream)
        {
            var toWriteInto = writer.GetMemory();
            var res = await stream.ReadAsync(toWriteInto);
            writer.Advance(res);
            FlushResult result = await writer.FlushAsync().ConfigureAwait(false);
            writer.Complete();
        }

        public override void Close()
        {
            reader.Complete();
            writer.Complete();
            pipe.Reset();
        }
    }
}