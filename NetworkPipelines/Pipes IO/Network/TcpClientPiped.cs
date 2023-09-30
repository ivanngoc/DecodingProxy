using System;
using System.Threading;
using IziHardGames.Libs.NonEngine.Memory;
using Microsoft.Extensions.Logging;
using Tuple = System.ValueTuple<System.Net.Sockets.Socket, System.Net.IPEndPoint, string>;

namespace IziHardGames.Libs.Networking.Pipelines
{
    [Obsolete]
    public class TcpClientPiped : PipedSocket
    {
        private IPoolReturn<TcpClientPiped>? pool;
        public uint Key { get; set; }
        public ILogger Logger { get => base.logger; }

        /// <summary>
        /// Counter of something. when rich 0 connection must be closed
        /// </summary>
        public int life;
        private bool isSetLife;

        public void Initilize(Tuple tuple)
        {
            BindSocket(tuple.Item1);
            this.ipEndPoint = tuple.Item2;
            Initilize(title);
        }

        public override void Close()
        {
            base.Close();
            pool?.Return(this);
            this.pool = default;
            Key = default;
        }

        public void BindToPool(IPoolReturn<TcpClientPiped> poolObjects)
        {
            this.pool = poolObjects ?? throw new NullReferenceException();
#if DEBUG
            Logger.LogInformation($"BindToPool {stopwatch.ElapsedMilliseconds}");
#endif      
        }

        public void SetLife(int max)
        {
            this.life = max;
            this.isSetLife = true;
        }

        public void ConsumeLife()
        {
            if (isSetLife)
            {
                Interlocked.Decrement(ref this.life);

                if (life <= 0)
                {
                    this.Close();
                }
            }
        }

        public virtual bool CheckData()
        {
            return CheckConnectIndirectly();
        }

        public TcpClientPipedSsl UpgradeConnection()
        {
            throw new System.NotImplementedException();
        }

    }
}