using IziHardGames.Libs.Async;
using IziHardGames.Libs.NonEngine.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Acceptor = System.Func<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped, System.Threading.CancellationToken, System.Threading.Tasks.Task<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>>;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class TcpServerPiped : PipedSocket, IDisposable
    {
        private readonly AsyncAutoResetCounter asyncCounter = new AsyncAutoResetCounter();
        public ConcurrentDictionary<uint, TcpClientPiped> pipedTcpClients = new ConcurrentDictionary<uint, TcpClientPiped>();
        public ConcurrentDictionary<uint, Task<TcpClientPiped>> tasks = new ConcurrentDictionary<uint, Task<TcpClientPiped>>();
        private CancellationTokenSource cts;
        private ILogger logger;
        private uint counter;

        public async Task Run(string address, int port, ILogger logger, Acceptor acceptor, CancellationToken token)
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
                Task.Run(() => AcceptClientAsync(handler, PoolObjectsConcurent<TcpClientPiped>.Shared, acceptor, cts.Token));
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

        public async Task AcceptClientAsync(Socket client, IPoolObjects<TcpClientPiped> poolObjects, Acceptor acceptClient, CancellationToken token)
        {
            asyncCounter.Increment();
            uint key = Interlocked.Increment(ref counter);
            var item = pipedTcpClients.GetOrAdd(key, (key) =>
            {
                var x = poolObjects.Rent();
                x.Key = key;
                return x;
            });
            logger.LogInformation($"Accepted tcp piped client guid:[{item.guid}]. Current clients:{pipedTcpClients.Count}. Tasks:{tasks.Count}");
            item.BindToPool(poolObjects);
            item.Bind(client, ipEndPoint);
            item.Init("Client");

            var run = Task.Run(async () =>
            {
                var t1 = acceptClient(item, token);
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
                item.ReportTime($"AcceptClientAsync finished task with status:{run.Status} exception:{run.Exception?.Message ?? "OK"}");
                while (!pipedTcpClients.TryRemove(item.Key, out item))
                {
                    new SpinWait().SpinOnce();
                }
                while (!tasks.TryRemove(key, out run))
                {
                    new SpinWait().SpinOnce();
                }
                item.Dispose();
                logger.LogInformation($"Client guid:{item.guid} disposed. Left clients:{pipedTcpClients.Count}. Tasks:{tasks.Count}");
                asyncCounter.Decrement();
                item.ReportTime($"AcceptClientAsync Disposed");
            });
            logger.LogInformation($"Task started: {run.Id} status:{run.Status}");
            await remove;
        }
    }
}