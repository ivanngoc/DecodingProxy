using IziHardGames.Libs.NonEngine.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Net;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.Networking.Contracts;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace IziHardGames.Libs.Networking.Servers
{
    public abstract class ServerBase<TClient>
        where TClient : IKey<uint>, IPerfTracker, IPoolBind<TClient>, IGuid, IDisposable
    {
        public abstract class AdapterClient : IClientHandlerAsync<TClient>
        {
            public abstract Task<TClient> HandleClientAsync(TClient handler, CancellationToken token = default);
        }

        public abstract class AdapterListener
        {
            public abstract Task<TClient> AcceptClientAsync(CancellationToken token = default);
            public abstract void Initilize();
            public abstract void Dispose();
        }
        private readonly AsyncAutoResetCounter asyncCounter = new AsyncAutoResetCounter();
        public ConcurrentDictionary<uint, TClient> clients = new ConcurrentDictionary<uint, TClient>();
        public ConcurrentDictionary<uint, Task<TClient>> tasks = new ConcurrentDictionary<uint, Task<TClient>>();
        private CancellationTokenSource cts;
        private ILogger logger;
        private uint counter;
        protected AdapterListener listener;
        protected AdapterClient clientHandler;

        public async Task Run(ILogger logger, CancellationToken token = default)
        {
            this.logger = logger;
            cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            token = cts.Token;
            listener.Initilize();

            while (!token.IsCancellationRequested)
            {
                var client = await listener.AcceptClientAsync().ConfigureAwait(false);
                var t1 = Task.Run(async () => await AcceptClientAsync(client, clientHandler, token));
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

        public async Task AcceptClientAsync(TClient client, IClientHandlerAsync<TClient> clientHandler, CancellationToken token)
        {
            asyncCounter.Increment();
            uint key = Interlocked.Increment(ref counter);
            client.Key = key;
#if DEBUG
            if (clients.ContainsKey(key))
            {
                throw new System.ArgumentException($"Key:{key} is alreaedy addeded");
            }
#endif
            if (!clients.TryAdd(key, client))
            {
                throw new ArgumentException($"Key [{client.Key}] is Already Exist");
            }
            logger.LogInformation($"Accepted tcp piped client guid:[{client.Guid}]. Current clients:{clients.Count}. Tasks:{tasks.Count}");

            var run = Task.Run(async () =>
            {
                var t1 = clientHandler.HandleClientAsync(client, token);
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
                if (!clients.TryRemove(item.Key, out item))
                {
                    throw new ArgumentOutOfRangeException($"CLient with key[{item.Key}] is not presented");
                }
                if (!tasks.TryRemove(key, out run))
                {
                    throw new ArgumentOutOfRangeException($"Task with key[{key}] is not presented");
                }
                item.Dispose();
                logger.LogInformation($"Client guid:{item.Guid} disposed. Left clients:{clients.Count}. Tasks:{tasks.Count}");
                asyncCounter.Decrement();
                item.ReportTime($"AcceptClientAsync Disposed");
            });
            logger.LogInformation($"Task started: {run.Id} status:{run.Status}");
            await remove;
        }
    }
}