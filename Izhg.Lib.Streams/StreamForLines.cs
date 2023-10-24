using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Streams
{
    public class StreamForLines : Stream
    {
        private Encoding encoding;
        private Stream innerStream;
        public static readonly byte[] rn = new byte[] { (byte)'\r', (byte)'\n' };
        public override bool CanRead { get => innerStream.CanRead; }
        public override bool CanSeek { get => innerStream.CanSeek; }
        public override bool CanWrite { get => innerStream.CanWrite; }
        public override long Length { get => innerStream.Length; }
        public override long Position { get => innerStream.Position; set => innerStream.Position = value; }

        public StreamForLines(Stream stream) : base()
        {
            this.innerStream = stream;
            this.encoding = Encoding.UTF8;
        }

        public override void Flush()
        {
            innerStream.Flush();
        }

        public async override Task FlushAsync(CancellationToken cancellationToken)
        {
            await innerStream.FlushAsync();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public async Task<string> ReadLineAsync(CancellationToken ct = default)
        {
            string result = string.Empty;
            var buffer = ArrayPool<byte>.Shared.Rent((1 << 20) * 4);
            int offset = default;
            REPEAT:
            // Read Byte
            int readed = await innerStream.ReadAsync(buffer, offset, 1, ct).ConfigureAwait(false);
            if (readed > 0)
            {
                //Console.WriteLine($"Readed char:{buffer[offset]}");
                int startIndex = Math.Clamp(offset - 1, 0, int.MaxValue);
                offset += readed;
                if (offset < 2) goto REPEAT;

                for (int i = 0; i < 2; i++)
                {
                    if (buffer[startIndex] == '\r' && buffer[startIndex + 1] == '\n')
                    {
                        result = encoding.GetString(buffer, 0, startIndex);
                        goto END;
                    }
                }
            }
            else
            {
                await Task.Delay(100);
            }
            goto REPEAT;
            END:
            ArrayPool<byte>.Shared.Return(buffer);
            return result;
        }
        public async Task WriteLineAsync(string line)
        {
            var bytes = encoding.GetBytes(line);
            await innerStream.WriteAsync(bytes).ConfigureAwait(false);
            await innerStream.WriteAsync(rn).ConfigureAwait(false);
        }
    }
}