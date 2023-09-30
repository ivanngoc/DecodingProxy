using System;

namespace IziHardGames.Libs.Networking.SocketLevel
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
            if (wrap.modifiers.ContainsKey(GetType())) throw new ArgumentException($"Modifier existed. Type:[{GetType().FullName}]");
#endif
            wrap.modifiers.Add(GetType(), this);
        }

        public virtual void InitilizeReverse()
        {
            wrap!.modifiers.Remove(GetType());
        }

        public virtual void Dispose()
        {
            wrap = default;
        }
    }
}