using IziHardGames.Core;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using Tuple = System.ValueTuple<System.Net.Sockets.Socket, System.Net.IPEndPoint, string>;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class TcpClientPiped : PipedSocket, IKey<uint>, IApplyControl, ICheckConnection, IInitializable<Tuple>, IPoolBind<TcpClientPiped>, IClient
    {
        private IPoolReturn<TcpClientPiped>? pool;
        public uint Key { get; set; }
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