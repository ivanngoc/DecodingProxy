using IziHardGames.Libs.ObjectsManagment;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Memory;
using System.Collections.Concurrent;
using IziHardGames.Libs.IO;
using System.Net.Sockets;

namespace IziHardGames.Proxy.Tcp
{
    public class ConnectionsToDomain : IDisposable
    {
        private readonly ConcurrentQueue<StalledData> stalled = new ConcurrentQueue<StalledData>();
        private readonly ConcurrentDictionary<uint, TcpClientPiped> activeConnections = new ConcurrentDictionary<uint, TcpClientPiped>();

        private IPoolReturn<ConnectionsToDomain> pool;
        public string Key { get; set; }
        private string host;
        private int port;

        /// <summary>
        /// Value for upgrading/downgrading existed connection
        /// </summary>
        private string version;
        private uint counter;
        private const int timeoutDefault = 60000;
        private int timeoutSpecified;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public async ValueTask<TcpClientPiped> GetOrCreate(string title)
        {
            REPEAT:
            if (stalled.TryDequeue(out StalledData data))
            {
                if (TryRevive(data))
                {
                    return data.client;
                }
                else
                {
                    goto REPEAT;
                }
            }
            counter++;
            var pool = PoolObjectsConcurent<TcpClientPiped>.Shared;
            var rent = pool.Rent();
            rent.BindToPool(pool);
            rent.Init();
            rent.BindTitle(title);
            await rent.ConnectAsync(host, port).ConfigureAwait(false);
            rent.key = counter;
            while (!activeConnections.TryAdd(counter, rent))
            {
                new SpinWait().SpinOnce();
            }
            return rent;
        }

        private bool TryRevive(StalledData stalledData)
        {
            var client = stalledData.client;
            if (client.CheckConnect())
            {
                stalledData.cts.Cancel();

                while (!activeConnections.TryAdd(counter, client))
                {
                    new SpinWait().SpinOnce();
                }
                return true;
            }
            return false;
        }

        private void Kill(TcpClientPiped client)
        {
            client.Dispose();
            CheckDispose();
        }

        public void Return(TcpClientPiped client)
        {
            while (!activeConnections.TryRemove(new KeyValuePair<uint, TcpClientPiped>(client.key, client)))
            {
                new SpinWait().SpinOnce();
            }
            if (client.CheckConnect())
            {
                MoveToStalled(client);
            }
            else
            {
                Kill(client);
            }
        }

        private void MoveToStalled(TcpClientPiped client)
        {
            int timeout;

            if (timeoutSpecified != default)
            {
                timeout = timeoutSpecified;
            }
            else
            {
                timeout = timeoutDefault;
            }

            var cts = new CancellationTokenSource();
            var watchdog = Task.Run(async () =>
              {
                  try
                  {
                      await Task.Delay(timeout, cts.Token);
                  }
                  catch (TaskCanceledException)
                  {
                      return;
                  }
                  Kill(client);
                  stalled.
              });

            StalledData stallData = new StalledData(client, cts, watchdog);
            stalled.Enqueue(stallData);
        }

        public void SetTimeout(int timeout)
        {
            this.timeoutSpecified = timeout;
        }

        public void Dispose()
        {
            host = default;
            port = default;
            pool.Return(this);
            pool = default;
            counter = default;
            //timeoutDefault = default;
            timeoutSpecified = default;
            cts.TryReset();

            if (stalled.Count > 0)
            {
                throw new NotImplementedException($"Connections must be properly closed");
            }
            Key = default;
        }

        public void Init(string host, int port)
        {
            this.host = host;
            this.port = port;
        }
        public void BindToPool(IPoolReturn<ConnectionsToDomain> pool)
        {
            this.pool = pool;
        }

        public void SetVersion(string version)
        {
            this.version = version;
        }
        public void UpgradeVersion()
        {
            throw new System.NotImplementedException();
        }
        public void DowngradeVersion()
        {
            throw new System.NotImplementedException();
        }

        public async Task Start()
        {

        }
        public void Stop()
        {

        }

        private void CheckDispose()
        {

        }

        private readonly struct StalledData
        {
            public readonly Task watchdog;
            public readonly TcpClientPiped client;
            public readonly CancellationTokenSource cts;

            public StalledData(TcpClientPiped client, CancellationTokenSource cts, Task watchdog) : this()
            {
                this.client = client;
                this.cts = cts;
                this.watchdog = watchdog;
            }
        }
    }
}