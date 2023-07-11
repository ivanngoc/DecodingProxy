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
    public class TcpClientPiped : PipedSocket, IKey<uint>, IApplyControl, ICheckConnection
    {
        private IPoolObjects<TcpClientPiped>? pool;
        public uint Key { get; set; }
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
            Key = default;
        }

        public void BindToPool<T>(IPoolObjects<T> poolObjects) where T : TcpClientPiped
        {
            this.pool = poolObjects as IPoolObjects<TcpClientPiped> ?? throw new NullReferenceException();
            Logger.LogInformation($"BindToPool {stopwatch.ElapsedMilliseconds}");
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
    }
}