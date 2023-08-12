using IziHardGames.Core;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Servers;
using IziHardGames.Libs.NonEngine.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Tuple = System.ValueTuple<System.Net.Sockets.Socket, System.Net.IPEndPoint, string>;


namespace IziHardGames.Libs.Networking.Pipelines
{
    /// <summary>
    /// <see cref="ServerBase{TClient}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TcpServerSocketBased<T> : IDisposable
        where T : IPoolBind<T>, IKey<uint>, IGuid, IInitializable<Tuple>, IDisposable, IGetLogger
    {
        private readonly AsyncAutoResetCounter asyncCounter = new AsyncAutoResetCounter();
        public ConcurrentDictionary<uint, T> clients = new ConcurrentDictionary<uint, T>();
        public ConcurrentDictionary<uint, Task<T>> tasks = new ConcurrentDictionary<uint, Task<T>>();
        private CancellationTokenSource cts;
        private ILogger logger;
        private uint counter;
        private Socket socket;
        private IPEndPoint ipEndPoint;

        public async Task Run(string address, int port, ILogger logger, IClientHandlerAsync<T> clientHandler, IPoolObjects<T> pool, CancellationToken token)
        {
            this.logger = logger;
            cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            token = cts.Token;

            IPHostEntry ipHostInfo = Dns.GetHostEntry(address);
            IPAddress ipAddress = ipHostInfo.AddressList.First(x => x.AddressFamily == AddressFamily.InterNetwork);
            IPEndPoint ipEndPoint = this.ipEndPoint = new IPEndPoint(ipAddress, port);
            var listener = this.socket = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Listen(100);

            while (!token.IsCancellationRequested)
            {
                var handler = await listener.AcceptAsync().ConfigureAwait(false);
                await Task.Run(() => AcceptClientAsync(handler, pool, clientHandler, cts.Token));
            }

            await asyncCounter.Await(0);
        }

        public async Task Stop()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
            }
        }

        public async Task AcceptClientAsync(Socket clientSocket, IPoolObjects<T> poolObjects, IClientHandlerAsync<T> clientHandler, CancellationToken token)
        {
            asyncCounter.Increment();
            uint key = Interlocked.Increment(ref counter);
            var item = clients.GetOrAdd(key, (key) =>
            {
                var x = poolObjects.Rent();
                x.Key = key;
                return x;
            });
            logger.LogInformation($"Accepted tcp piped client guid:[{item.Guid}]. Current clients:{clients.Count}. Tasks:{tasks.Count}");

            item.BindToPool(poolObjects);
            item.Initilize((clientSocket, ipEndPoint, "Client"));
            var clientLogger = item.Logger as IPerfTracker ?? throw new NullReferenceException();

            var run = Task.Run(async () =>
            {
                var t1 = clientHandler.HandleClientAsync(item, token);
                if (!tasks.TryAdd(key, t1))
                {
                    throw new ArgumentException($"Task with id {t1.Id} is already existed");
                }
                return await t1;
            }, token);

            var remove = Task.Run(async () =>
            {
                await run;
                var item = run.Result;
                logger.LogInformation($"Taks Completed: id:{run.Id} status:{run.Status} exception:{run.Exception?.Message ?? "OK"}");
                clientLogger.ReportTime($"AcceptClientAsync finished task with status:{run.Status} exception:{run.Exception?.Message ?? "OK"}");
                while (!clients.TryRemove(item.Key, out item))
                {
                    new SpinWait().SpinOnce();
                }
                while (!tasks.TryRemove(key, out run))
                {
                    new SpinWait().SpinOnce();
                }
                item.Dispose();
                logger.LogInformation($"Client guid:{item.Guid} disposed. Left clients:{clients.Count}. Tasks:{tasks.Count}");
                asyncCounter.Decrement();
                clientLogger.ReportTime($"AcceptClientAsync Disposed");
            });
            logger.LogInformation($"Task started: {run.Id} status:{run.Status}");
            await remove;
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}