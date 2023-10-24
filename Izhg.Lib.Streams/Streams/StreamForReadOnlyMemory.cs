using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Streaming;
using IziHardGames.Libs.Streams.Attributes;

namespace IziHardGames.Libs.Streams
{
    [Bidirectional, FakeWrite]
    public class StreamForReadOnlyMemory : ReusableStream
    {
        public override bool CanRead { get => true; }
        public override bool CanSeek { get => true; }
        public override bool CanWrite { get => true; }
        public override long Length { get => bytes.Length; }
        public override long Position { get; set; }

        protected ReadOnlyMemory<byte> bytes;

        public void Initilize(ReadOnlyMemory<byte> memory)
        {
            bytes = memory;
        }
        public override void Flush()
        {

        }

        #region Read
        public override int Read(byte[] buffer, int offset, int count)
        {
            int toCopy = count > bytes.Length ? bytes.Length : count;
            bytes.CopyTo(buffer.AsMemory().Slice(offset, toCopy));
            bytes = bytes.Slice(toCopy);
            return toCopy;
        }
        public override int Read(Span<byte> buffer)
        {
            int count = buffer.Length;
            int toCopy = count > bytes.Length ? bytes.Length : count;
            bytes.Span.Slice(0, toCopy).CopyTo(buffer);
            bytes = bytes.Slice(toCopy);
            return toCopy;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.FromResult(Read(buffer, offset, count));
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(Read(buffer.Span));
        }
        public override int ReadByte()
        {
            int result = bytes.Span[0];
            bytes = bytes.Slice(1);
            return result;
        }
        #endregion
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }
        #region Write
        public override void Write(byte[] buffer, int offset, int count)
        {

        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {

        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ValueTask.CompletedTask;
        }
        public override void WriteByte(byte value)
        {

        }
        #endregion

    }
}
