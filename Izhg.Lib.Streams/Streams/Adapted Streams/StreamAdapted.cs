using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.Buffers.Readers;
using IziHardGames.Libs.Buffers.Sources;
using IziHardGames.Libs.Buffers.Writers;
using IziHardGames.Libs.Streaming;
using IziHardGames.Libs.Streams.Attributes;

namespace IziHardGames.Libs.Streams
{
    public class StreamAdapted : ReusableStream
    {
        protected AdapterForWrite? writer;
        protected AdapterForRead? reader;
        protected SourceAdapter? sourceAdapterReader;
        protected SourceAdapter? sourceAdapterWriter;

        public override bool CanRead { get => sourceAdapterReader?.CanRead ?? false; }
        public override bool CanWrite { get => sourceAdapterWriter?.CanWrite ?? false; }
        public override bool CanSeek { get => throw new System.NotImplementedException(); }
        public override long Length { get => throw new System.NotImplementedException(); }
        public override long Position { get; set; }

        public void Initilize(Stream streamRead, Stream streamWrite)
        {
            InitilizeReader(streamRead);
            InitilizeWriter(streamWrite);
        }


        public void Initilize(in ReadOnlyMemory<byte> readOnlyMemory, Stream stream)
        {
            InitilizeReader(in readOnlyMemory);
            InitilizeWriter(stream);
        }

        [Bidirectional(Type = ETransmitType.ReadWrite)]
        public void Initilize(in ReadOnlyMemory<byte> readOnlyMemory, Memory<byte> write)
        {
            InitilizeReader(in readOnlyMemory);
            InitilizeWriter(write);
        }

        [OneDirection(Type = ETransmitType.OnlyRead)]
        public void Initilize(in ReadOnlyMemory<byte> readOnlyMemory)
        {
            InitilizeReader(in readOnlyMemory);
        }

        public void Initilize(byte[] bytes)
        {
            throw new System.NotImplementedException();
        }
        public void Initilize(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        private void InitilizeReader(Stream streamRead)
        {
            throw new NotImplementedException();
        }
        private void InitilizeReader(in ReadOnlyMemory<byte> readOnlyMemory)
        {
            SourceAdapterForReadOnlyMemory source = SourceAdapterForReadOnlyMemory.GetOrCreate();
            source.source = readOnlyMemory;
            this.sourceAdapterReader = source;

            var reader = this.reader = AdapterForReadFromReadOnlyMemory.GetOrCreate();
            reader.SetSource(source);
        }
        private void InitilizeWriter(Memory<byte> readOnlyMemory)
        {
            SourceAdapterForMemory source = SourceAdapterForMemory.GetOrCreate();
            source.source = readOnlyMemory;
            this.sourceAdapterWriter = source;

            var writer = this.writer = AdapterForWriteToMemory.GetOrCreate();
            writer.SetSource(source);
        }
        private void InitilizeWriter(Stream stream)
        {
            SourceAdapterForStream source = SourceAdapterForStream.GetOrCreate();
            source.source = stream;
            this.sourceAdapterWriter = source;

            var writer = this.writer = AdapterForWriteToStream.GetOrCreate();
            writer.SetSource(source);
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

        #region Writers
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteByte(byte value)
        {
            throw new System.NotImplementedException();
        }
        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await writer!.WriteAsync(new Memory<byte>(buffer, offset, count)).ConfigureAwait(false);
        }
        public async override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await writer!.WriteAsync(buffer).ConfigureAwait(false);
        }
        #endregion

        public override void Close()
        {
            base.Close();
            writer!.Dispose();
            reader!.Dispose();
            sourceAdapterReader!.Dispose();

            writer = default;
            reader = default;
            sourceAdapterReader = default;
        }
    }
}
