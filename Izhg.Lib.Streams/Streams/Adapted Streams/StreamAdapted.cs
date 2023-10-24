using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.Streaming;

namespace IziHardGames.Libs.Streams
{
    public class StreamAdapted : ReusableStream
    {
        protected AdapterForWrite? writer;
        protected AdapterForRead? reader;
        protected SourceAdapter? sourceAdapter;

        public override bool CanRead { get => sourceAdapter.CanRead; }
        public override bool CanSeek { get => sourceAdapter.CanSeek; }
        public override bool CanWrite { get => sourceAdapter.CanWrite; }
        public override long Length { get => sourceAdapter.Length; }
        public override long Position { get => sourceAdapter.Position; set => sourceAdapter.Position = value; }

        public void Initilize(ReadOnlyMemory<byte> readOnlyMemory)
        {
            SourceAdapterForReadOnlyMemory source = SourceAdapterForReadOnlyMemory.GetOrCreate();
            source.source = readOnlyMemory;
            this.sourceAdapter = source;
            var reader = this.reader = AdaptedReaderForReadOnlyMemory.GetOrCreate();
            reader.SetSource(source);
        }
        public void Initilize(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
        public void Initilize(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public override void Flush()
        {

        }

        #region Reads
        public override int ReadByte()
        {
            throw new System.NotImplementedException();
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            return reader!.Read(buffer, offset, count);
        }
        public override int Read(Span<byte> buffer)
        {
            return reader!.Read(in buffer);
        }
        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return await reader!.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
            writer!.Dispose();
            reader!.Dispose();
            sourceAdapter!.Dispose();

            writer = default;
            reader = default;
            sourceAdapter = default;
        }
    }
}
