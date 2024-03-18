using System;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SocketWriterPiped : SocketWriter, IPoolBind<SocketWriterPiped>
    {
        protected IPoolReturn<SocketWriterPiped>? pool;

        public void BindToPool(IPoolReturn<SocketWriterPiped> pool)
        {
            this.pool =pool;
        }
    }
}