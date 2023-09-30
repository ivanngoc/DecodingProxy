using System;
using System.Buffers;
using IziHardGames.Libs.Networking.Options;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public abstract class SocketReaderInterceptor : IDisposable
    {
        public abstract EReadStatus Intercept(in ReadOnlySequence<byte> sequence);
        public abstract EReadStatus Intercept(in Memory<byte> mem);

        public SocketReader source;

        public virtual void Dispose()
        {
            source = default;
        }
        public virtual void Initilize(SocketReader socketReader)
        {
            this.source = socketReader;
        }
    }
}