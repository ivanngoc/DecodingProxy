using System;
using System.Net.Sockets;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public abstract class SocketProcessor : IDisposable
    {
        protected bool isDisposed = true;
        protected SocketWrap wrap;

        public virtual void Initilize(SocketWrap wrap)
        {
            if (!isDisposed) throw new ObjectDisposedException("Object must be disposed for use");
            isDisposed = false;
            this.wrap = wrap;
        }

        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("Object already disposed");
            isDisposed = true;
            wrap = default;
        }
    }
}