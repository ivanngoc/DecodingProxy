// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class TcpClientPiped : PipedSocket
    {
        private IPoolObjects<TcpClientPiped>? pool;
        public uint key;
        /// <summary>
        /// Counter of something. when rich 0 connection must be closed
        /// </summary>
        public int life;
        private bool isSetLife;

        public void Bind(Socket socket, IPEndPoint ipEndPoint)
        {
            Logger.LogInformation($"Bind {stopwatch.ElapsedMilliseconds}");
            BindSocket(socket);
            this.ipEndPoint = ipEndPoint;
        }

        public override void Close()
        {
            base.Close();
            pool.Return(this);
            pool = default;
            key = default;
        }

        public void BindToPool(IPoolObjects<TcpClientPiped> poolObjects)
        {
            this.pool = poolObjects;
            Logger.LogInformation($"BindToPool {stopwatch.ElapsedMilliseconds}");
        }

        internal void SetLife(int max)
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
    }
}