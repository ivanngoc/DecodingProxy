using System;
using System.IO;
using System.Net.Sockets;
using IziHardGames.Libs.Streams.Contracts;

namespace IziHardGames.Libs.Streams
{
    /// <summary>
    /// Объединяет 2 <see cref="Stream"/> так, что запись в первый поток будет считываться со второго, а запись во второй поток будет считываться с первого.
    /// Может использоваться для тестов между клиентом и сервером вместо <see cref="NetworkStream"/>
    /// </summary>
    public class StreamsCross : Stream
    {
        public override bool CanRead { get => true; }
        public override bool CanWrite { get => true; }
        public override bool CanSeek { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        private Stream left;
        private Stream right;

        public void Initilize(Stream left, Stream right)
        {
            this.left = left;
            this.right = right;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        #region Reads
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override int Read(Span<byte> buffer)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #region Writes
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
