using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Buffers.Attributes;
using IziHardGames.Libs.Streams.Contracts;

namespace IziHardGames.Libs.Streams
{

    public class SocketStream : Stream, IStream
    {
        public override bool CanRead { get => true; }
        public override bool CanSeek { get => throw new System.NotImplementedException(); }
        public override bool CanWrite { get => true; }
        public override long Length { get => throw new System.NotImplementedException(); }
        public override long Position { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        protected Socket? socket;
        protected bool isDisposed = true;

        public void Initilize(Socket socket)
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed for Initilization");
            isDisposed = false;
            this.socket = socket;
        }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }


        [ZeroReadBlock]
        public override int Read(byte[] buffer, int offset, int count)
        {
            REPEAT:
            int readed = socket!.Receive(buffer, offset, count, SocketFlags.None);
            if (readed > 0)
            {
                //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Readed From Socket:{readed}. Connection: {socket.Connected}");
                return readed;
            }
            goto REPEAT;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        [ZeroReadBlock]
        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            int readed = default;
            while (true)
            {
                readed = await socket!.ReceiveAsync(buffer, SocketFlags.None, ct).ConfigureAwait(false);
                if (readed > 0)
                {
                    //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Readed From Socket:{readed}");
                    return readed;
                }
                else
                {
                    await Task.Delay(ConstantsForStream.Timeouts.DEFAULT_ZERO_READ_TIMEOUT, ct).ConfigureAwait(false);
                }
            }
        }
        public override int ReadByte()
        {
            throw new System.NotImplementedException();
        }
        public override int Read(Span<byte> buffer)
        {
            throw new System.NotImplementedException();
        }



        public override void Write(byte[] buffer, int offset, int count)
        {
            //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Writed:{count}");
            socket!.Send(buffer, offset, count, SocketFlags.None);
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        public async override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var writed = await socket!.SendAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);
            //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Gived/Writed:{buffer.Length}/{writed}");
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteByte(byte value)
        {
            throw new System.NotImplementedException();
        }
        public override void Close()
        {
            if (isDisposed) throw new ObjectDisposedException($"Object is already disposed");
        }
    }
}
