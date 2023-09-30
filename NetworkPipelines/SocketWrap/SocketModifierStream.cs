using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketModifierStream : SocketWrapModifier, IPoolBind<SocketModifierStream>
    {
        private readonly SocketReaderForStream reader = new SocketReaderForStream();
        private readonly SocketWrtierForStream writer = new SocketWrtierForStream();
        private readonly SocketWrapStream stream = new SocketWrapStream();

        private IPoolReturn<SocketModifierStream> pool;

        public Stream Stream => stream;

        public void BindToPool(IPoolReturn<SocketModifierStream> pool)
        {
            this.pool = pool;
        }
        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            SetStream(this.stream);
            reader.Initilize(wrap);
            writer.Initilize(wrap);
        }
        public override void Dispose()
        {
            base.Dispose();
            pool.Return(this);
            pool = default;
            stream.Dispose();
            reader.Dispose();
            writer.Dispose();
        }
        private void SetStream(Stream stream)
        {
            reader.SetStream(stream);
            writer.SetStream(stream);
        }
    }

    internal class SocketWrapStream : Stream, ISocketReaderBind, ISocketWriterBind
    {
        public override bool CanSeek { get => throw new System.NotImplementedException(); }
        public override bool CanRead { get => true; }
        public override bool CanWrite { get => true; }
        public override long Length { get => throw new System.NotImplementedException(); }
        public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private SocketReader? sourceReader;
        private SocketWriter? sourceWriter;

        public void Initilize(SocketWrap wrap)
        {
            wrap.Reader.Bind(this);
            wrap.Writer.Bind(this);
        }
        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return sourceReader!.TransferTo(buffer, offset, count);
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await this.sourceReader!.TransferToAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        public override int Read(Span<byte> buffer)
        {
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
            int writed = sourceWriter!.Write(buffer, offset, count);
            if (writed != count) throw new NotImplementedException($"Writed less than expected");
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return base.WriteAsync(buffer, cancellationToken);
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            base.Write(buffer);
        }
        public override void WriteByte(byte value)
        {
            base.WriteByte(value);
        }
        public override void Close()
        {
            base.Close();
            sourceReader = default;
            sourceWriter = default;
        }

        public void SetWrtier(SocketWriter writer)
        {
            sourceWriter = writer;
        }

        public void SetReader(SocketReader reader)
        {
            sourceReader = reader;
        }
    }

    internal class SocketWrtierForStream : SocketWriter
    {
        private Stream? stream;
        public void SetStream(Stream stream)
        {
            this.stream = stream;
        }
        public override void Dispose()
        {
            base.Dispose();
            stream = default;
        }
    }

    internal class SocketReaderForStream : SocketReader
    {
        private Stream? stream;
        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
        }
        public override ValueTask<int> TransferToAsync(Memory<byte> mem, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public override int TransferTo(byte[] array, int offset, int length)
        {
            throw new NotImplementedException();
        }
        public override void Dispose()
        {
            base.Dispose();
            stream = default;
        }
        public void SetStream(Stream stream)
        {
            this.stream = stream;
        }
    }
}