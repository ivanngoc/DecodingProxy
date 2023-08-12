using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketWriterPiped : SocketReader, IPoolBind<SocketWriterPiped>
    {
        protected IPoolReturn<SocketWriterPiped>? pool;

        public void BindToPool(IPoolReturn<SocketWriterPiped> pool)
        {
            this.pool =pool;
        }
    }
}