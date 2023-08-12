using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.Streams.Contracts;

namespace IziHardGames.Libs.Streams
{
    public class SocketStream : Stream, IStream
    {
        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

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
            throw new NotImplementedException();
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

        public override void Close()
        {
            if (isDisposed) throw new ObjectDisposedException($"Object is already disposed");
        }
    }
}
