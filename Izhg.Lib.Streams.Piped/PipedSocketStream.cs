using System.IO.Pipelines;
using System.Net.Sockets;
using IziHardGames.Libs.Pipelines;

namespace IziHardGames.Libs.Streams.Piped
{
    public class PipedSocketStream : Stream
    {
        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        protected readonly Pipe pipe;
        protected readonly PipeReader reader;
        protected readonly PipeWriter writer;
        protected Socket socket;
        protected bool isDisposed = true;
        public static PipeOptions optionsByDefault => SharedPipes.pipeOptions;

        public PipedSocketStream() : base()
        {
            pipe = new Pipe(optionsByDefault);
            reader = pipe.Reader;
            writer = pipe.Writer;
        }

        public void Initilize(Socket socket)
        {
            if (!isDisposed) throw new ObjectDisposedException("object must be disposed for initilization");
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
            if (isDisposed) throw new ObjectDisposedException("Object already disposed");
            isDisposed = true;
            socket.Dispose();
            socket = default;

            writer.Complete();
            reader.Complete();
            pipe.Reset();
        }
    }
}