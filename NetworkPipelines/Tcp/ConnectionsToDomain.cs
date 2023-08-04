using IziHardGames.Core;
using IziHardGames.Libs.Networking.Clients;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.Tcp
{
    public class ConnectionsToDomain : IDisposable
    {
        protected DateTime startTime;
        protected DateTime checkTime;
        protected string? host;
        protected int port;
        protected bool isDisposed = true;
        /// <summary>
        /// Value for upgrading/downgrading existed connection
        /// </summary>
        protected string version = string.Empty;
        protected static uint counterForId;
        protected uint countRevives;
        public IChangeNotifier<IConnectionData> monitor;

        public virtual void Dispose()
        {
            Console.WriteLine($"ConnectionToDomain {host}:{port} disposed");
            if (isDisposed) throw new ObjectDisposedException($"Object is already disposed");
            isDisposed = true;

            version = string.Empty;

            startTime = default;
            checkTime = default;
            host = null;
            port = default;
            countRevives = default;
            monitor = default;
        }
    }

    /*
Сценарии завершения подключения:
1. Обмен сообщениями завершен. Подключение возвращается и помещается в свободный список
2. Помещенное в свободный список соединение истекает по таймауту. Соединение закрывается штатно и управляемо
3. Удаленный сервер закрывает соединение. Поток обмена сообщениями дожидается последний рид и далее пункт 1.
*/
    public class ConnectionsToDomain<T1> : ConnectionsToDomain, IKey<string>, IHub<T1>
        where T1 : IInitializable<string>, IPoolBind<T1>, IClient, IConnectorTcp, IKey<uint>, IDisposable, IConnection, IConnectionData
    {
        private readonly ConcurrentDictionary<uint, T1> activeConnections = new ConcurrentDictionary<uint, T1>();
        private StalledData? head;

        private IPoolReturn<ConnectionsToDomain<T1>>? pool;
        public string Key { get; set; }

        private const int timeoutDefault = 60000;
        private int timeoutSpecified;

        private Action<ConnectionsToDomain<T1>> returnToManager;
        /// <summary>
        /// COunter for sync. In case of: manager have active CTD. thread A get that object. Meanwhile thread B executing Kill(). Thread A enter PullHead(). Thread B killed CTD. 
        /// Thread A don'tknow about CTD is disposed and continue to do as it alive. <br/>
        /// </summary>
        private uint usage;

        public virtual async ValueTask<T1> GetOrCreateAsync(string title, IPoolObjects<T1> pool)
        {
            if (!TryGet(out T1 result))
            {
                result = await CreateAsync(title, pool);
            }
            result.SetState(EConnectionState.Active);
            monitor.OnUpdate(result);
            return result;
        }

        protected async virtual ValueTask<T1> CreateAsync(string title, IPoolObjects<T1> pool)
        {
            var id = Interlocked.Increment(ref counterForId);
            var rent = pool.Rent();
            rent.BindToPool(pool);
            rent.Initilize(title);
            rent.Key = id;
#if DEBUG
            if (activeConnections.ContainsKey(id))
            {
                throw new System.ArgumentException($"Key:{id} is alreaedy addeded");
            }
#endif
            if (!activeConnections.TryAdd(id, rent))
            {
                throw new ArgumentException($"Key [{id}] is Already Exist");
            }
            // сначала добавить в список активных подключений и лишь затем подключаться.
            try
            {
                await rent.ConnectAsyncTcp(host, port).ConfigureAwait(false);
                rent.RunWriterLoop();
            }
            catch (SocketException)
            {
                while (!activeConnections.TryRemove(new KeyValuePair<uint, T1>(rent.Key, rent)))
                {
                    new SpinWait().SpinOnce();
                }
                rent.Dispose();
                throw new NotImplementedException();
            }
            rent.ReportTime($"ConnectionsToDomain GetOrCreate completed with create id={id}");
            monitor.OnAdd(rent);
            return rent;
        }
        protected virtual bool TryGet(out T1 client)
        {
            REPEAT:
            var data = PullHead();
            if (data != null)
            {
                client = data.client;
                if (TryReviveOtherwiseKill(data))
                {
                    client.ReportTime($"ConnectionsToDomain GetOrCreate completed with get stall");
                    return true;
                }
                else
                {
                    goto REPEAT;
                }
            }
            client = default;
            return false;
        }
        private StalledData PullHead()
        {
            if (head == null) return default;
            StalledData result;
            lock (this)
            {
                result = head;
                head = head.next;
                if (head != null) head.previous = null;
            }
            return result;
        }

        private bool TryReviveOtherwiseKill(StalledData stalledData)
        {
            stalledData.client.ReportTime($"TryRevive Started:{host}:{port}");
            var client = stalledData.client;

            if (client.CheckConnect())
            {
                stalledData.cts.Cancel();

                if (!activeConnections.TryAdd(client.Key, client))
                {
                    throw new ArgumentException($"Key [{client.Key}] is Already Exist");
                }
                stalledData.client.ReportTime($"TryRevive Succeded:{host}:{port}");
                // stallData.cts was canceled. Need to reset cts              
                stalledData.Dispose();
                this.countRevives++;
                monitor.OnUpdate(client);
                return true;
            }
            stalledData.client.ReportTime($"TryRevive Failed:{host}:{port}. Connection Killed");
            Kill(client);
            return false;
        }

        private void Kill(T1 client)
        {
            monitor.OnRemove(client);
            client.ReportTime($"Killing started {host}:{port}. clientID:{client.Key}");
            var task = client.StopWriteLoop();
            task.Wait();
            client.ReportTime($"ConnectionsToDomain: Connection Killed. host {host}:{port}. clientID:{client.Key}");
            client.Dispose();
            CheckDispose();
            client.ReportTime($"ConnectionsToDomain: IsDisposed:{isDisposed}");
        }

        protected virtual void ReturnToManager()
        {
            this.returnToManager(this);
        }
        public void Return(T1 client)
        {
            Console.WriteLine($"ConnectionsToDomain: id:{client.Key} {host}:{port} Return active connection");
            client.ReportTime($"ConnectionsToDomain: id:{client.Key} {host}:{port} Return active connection");

            if (!activeConnections.ContainsKey(client.Key))
            {
                throw new NotImplementedException("Multiple return detected?");
            }
            while (!activeConnections.TryRemove(new KeyValuePair<uint, T1>(client.Key, client)))
            {
                new SpinWait().SpinOnce();
            }
            client.ReportTime($"ConnectionsToDomain: Removed from active connections");

            if (client.CheckConnect())
            {
                MoveToStalled(client);
            }
            else
            {
                Kill(client);
            }
        }

        private void MoveToStalled(T1 client)
        {
            client.ReportTime($"Moved to Stall: {host}:{port}");
            client.SetState(EConnectionState.Stalled);

            int timeout;

            if (timeoutSpecified != default)
            {
                timeout = timeoutSpecified;
            }
            else
            {
                timeout = timeoutDefault;
            }

            StalledData stallData = StalledData.Get(this);
            var watchdog = Task.Run(async () =>
              {
                  try
                  {
                      client.ReportTime($"ConnectionToDomain:{host}:{port}. Death Timer is Started:timeout:{timeout}. TokenCanceled:{stallData.cts.Token.IsCancellationRequested}");
                      await Task.Delay(timeout, stallData.cts.Token);
                  }
                  catch (TaskCanceledException)
                  {
                      client.ReportTime($"Stalled connection {host}:{port} Death Timer is Stopped");
                      return;
                  }
                  client.ReportTime($"Stalled connection is out of time: {host}:{port}");
                  stallData.RemoveFromChain();
                  // stallData.cts.token is not used no need to stallData.Reset()
                  stallData.Dispose();
                  Kill(client);
              });
            stallData.Init(client, watchdog);

            // расположить в конце очереди
            lock (this)
            {
                if (head != null)
                {
                    StalledData current = head;
                    while (current.next != null)
                    {
                        current = current.next;
                    }
                    stallData.SetAfter(current);
                }
                else
                {
                    head = stallData;
                }
            }
        }

        public void SetTimeout(int timeout)
        {
            this.timeoutSpecified = timeout;
        }

        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;

            //timeoutDefault = default;
            timeoutSpecified = default;

            if (head != null)
            {
                throw new NotImplementedException($"Connections must be properly closed");
            }
            Key = default;
            this.returnToManager = default;
        }

        public void UpdateAddress(string host, int port)
        {
            this.host = host;
            this.port = port;
        }
        public void BindToPool(IPoolReturn<ConnectionsToDomain<T1>> pool)
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

        public virtual void Start()
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed to reuse or used right after creation");
            isDisposed = false;
            startTime = DateTime.Now;
        }
        public void Stop()
        {

        }

        private void CheckDispose()
        {
            bool isDisposeNeeded = false;
            this.checkTime = DateTime.Now;
            lock (this)
            {
                if (head == null && activeConnections.Count == 0 && usage == 0)
                {
                    isDisposeNeeded = true;
                }
                if (isDisposeNeeded)
                {
                    Stop();
                    ReturnToManager();
                }
            }
        }

        public void RegistRuturnToManager(Action<ConnectionsToDomain<T1>> returnToManager)
        {
            this.returnToManager = returnToManager!;
        }

        public void Use()
        {
            Interlocked.Increment(ref usage);
        }
        public void Unuse()
        {
            Interlocked.Decrement(ref usage);
        }

        /// <summary>
        /// Двусвязный список
        /// </summary>
        private class StalledData : IDisposable
        {
            public CancellationTokenSource cts = new CancellationTokenSource();

            public Task watchdog;
            public T1 client;
            public StalledData? next;
            public StalledData? previous;
            private IPoolReturn<StalledData> pool;
            private ConnectionsToDomain<T1> ctd;

            internal static StalledData Get(ConnectionsToDomain<T1> ctd)
            {
                var rent = PoolObjectsConcurent<StalledData>.Shared.Rent();
                // cancel operation might be still running. Obtain New cts if needed
                rent.Reset();
                rent.Bind(PoolObjectsConcurent<StalledData>.Shared, ctd);
                return rent;
            }

            private void Bind(IPoolReturn<StalledData> pool, ConnectionsToDomain<T1> ctd)
            {
                this.ctd = ctd;
                this.pool = pool;
            }

            public StalledData Init(T1 client, Task watchdog)
            {
                this.client = client;
                this.watchdog = watchdog;
                return this;
            }

            public void Reset()
            {
                if (!cts.TryReset())
                {
                    cts = new CancellationTokenSource();
                }
                watchdog = null;
            }
            public void Dispose()
            {
                pool.Return(this);
                pool = default;
                client = default;
                ctd = default;
                next = default;
                previous = default;
            }

            internal void RemoveFromChain()
            {
                client.ReportTime("Removed from chain of stalled connections");

                lock (ctd)
                {
                    if (this == ctd.head)
                    {
                        ctd.head = next;
                    }
                    else
                    {
                        if (next != null)
                        {   // сдвигаем следующий элемент на место этого
                            this.next.SetAfter(this.previous);
                        }
                        else
                        {   // отрубаем конец
                            previous.next = default;
                        }
                    }
                }
            }
            internal void SetAfter(StalledData prev)
            {
                prev.next = this;
                this.previous = prev;
            }
        }
    }
}