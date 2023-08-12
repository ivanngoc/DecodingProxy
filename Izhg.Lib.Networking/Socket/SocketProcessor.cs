using System;
using System.Net.Sockets;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public abstract class SocketProcessor : IDisposable
    {
        protected Socket? socket;
        protected bool isDisposed = true;

        public void Initilize(Socket socket)
        {
            if (!isDisposed) throw new ObjectDisposedException("Object must be disposed for use");
            this.socket = socket;
        }

        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("Object already disposed");
            isDisposed = true;
            socket = default;
        }
    }
}