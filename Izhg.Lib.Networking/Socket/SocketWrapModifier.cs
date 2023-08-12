using System;
using System.Linq;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Proxy.Tcp
{
    /// <summary>
    /// Approach modify. For example NetworkStream or Pipeline or Combined
    /// </summary>
    public abstract class SocketWrapModifier : IDisposable
    {
        protected SocketWrap? wrap;
        public virtual void Initilize(SocketWrap wrap)
        {
            this.wrap = wrap;
#if DEBUG
            if (wrap.modifiers.ContainsKey(this.GetType())) throw new ArgumentException($"Modifier existed. Type:[{GetType().FullName}]");
#endif
            wrap.modifiers.Add(this.GetType(), this);
        }
        public virtual void Dispose()
        {
            wrap = default;
        }
    }
}