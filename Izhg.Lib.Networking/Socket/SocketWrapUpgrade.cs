using System;
using IziHardGames.Libs.Networking.SocketLevel;

namespace IziHardGames.Proxy.Tcp
{
    /// <summary>
    /// Protocol Upgrade. Can modify the way to read/write data
    /// </summary>
    public abstract class SocketWrapUpgrade : IDisposable
    {
        protected SocketWrap? wrap;
        protected bool isDisposed = true;
        public SocketWrapLogger Logger => wrap.logger;

        public void Initilize(SocketWrap wrap)
        {
            this.wrap = wrap;
            Initilize();
        }
        protected virtual void Initilize()
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed for initilization");
            isDisposed = false;
        }
        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException($"Object already disposed");
            isDisposed = true;
            wrap = default;
        }
        public abstract void ApplyTo(SocketWrap socketWrap);
    }
}