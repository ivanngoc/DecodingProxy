using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Networking.SocketLevel
{

    public class SocketModifierBufferDefault : SocketWrapModifier, IPoolBind<SocketModifierBufferDefault>
    {
        private IPoolReturn<SocketModifierBufferDefault> pool;
        public readonly SocketReaderBuffered<SocketBufferDefault> reader = new SocketReaderBuffered<SocketBufferDefault>();

        public void BindToPool(IPoolReturn<SocketModifierBufferDefault> pool)
        {
            this.pool = pool;
        }
        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            reader.Initilize(wrap);
        }
        public override void InitilizeReverse()
        {
            base.InitilizeReverse();
            reader.Dispose();
        }
    }
}