using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace IziHardGames.Proxy.Tcp
{
    /*
     Сценарии завершения подключения:
     1. Обмен сообщениями завершен. Подключение возвращается и помещается в свободный список
     2. Помещенное в свободный список соединение истекает по таймауту. Соединение закрывается штатно и управляемо
     3. Удаленный сервер закрывает соединение. Поток обмена сообщениями дожидается последний рид и далее пункт 1.
     */
    public class ConnectionsToDomain<T1> : IKey<string>, IDisposable
        where T1 : TcpClientPiped
    {
        private readonly ConcurrentDictionary<uint, T1> activeConnections = new ConcurrentDictionary<uint, T1>();
        private StalledData? head;

        private IPoolReturn<ConnectionsToDomain<T1>>? pool;
        public string Key { get; set; }
        protected string host;
        protected int port;

        /// <summary>
        /// Value for upgrading/downgrading existed connection
        /// </summary>
        private string version;
        private uint counter;
        private const int timeoutDefault = 60000;
        private int timeoutSpecified;
        private DateTime startTime;
        private DateTime checkTime;
        private bool isDisposed = true;
        private Action<ConnectionsToDomain<T1>> returnToManager;
        /// <summary>
        /// COunter for sync. In case of: manager have active CTD. thread A get that object. Meanwhile thread B executing Kill(). Thread A enter PullHead(). Thread B killed CTD. 
        /// Thread A don'tknow about CTD is disposed and continue to do as it alive. <br/>
        /// </summary>
        private uint usage;

        public virtual async ValueTask<T1> GetOrCreate(string title, IPoolObjects<T1> pool)
        {
            REPEAT:
            var data = PullHead();
            if (data != null)
            {
                var client = data.client;
                if (TryRevive(data))
                {
                    client.ReportTime($"ConnectionsToDomain GetOrCreate completed with get stall");
                    return client;
                }
                else
                {
                    goto REPEAT;
                }
            }
            var id = Interlocked.Increment(ref counter);
            var rent = pool.Rent();
            rent.BindToPool(pool);
            rent.Init(title);
            rent.Key = id;
            while (!activeConnections.TryAdd(id, rent))
            {
                new SpinWait().SpinOnce();
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
            return rent;
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

        private bool TryRevive(StalledData stalledData)
        {
            stalledData.client.ReportTime($"TryRevive Started:{host}:{port}");
            var client = stalledData.client;
            if (client.CheckConnect())
            {
                stalledData.cts.Cancel();

                while (!activeConnections.TryAdd(client.Key, client))
                {
                    new SpinWait().SpinOnce();
                }
                stalledData.client.ReportTime($"TryRevive Succeded:{host}:{port}");
                // stallData.cts was canceled. Need to reset cts              
                stalledData.Dispose();
                return true;
            }
            stalledData.client.ReportTime($"TryRevive Failed:{host}:{port}");
            return false;
        }

        private void Kill(T1 client)
        {
            client.ReportTime($"Killing started {host}:{port}. clientID:{client.Key}");
            var task = client.StopWriteLoop();
            task.Wait();
            client.ReportTime($"ConnectionsToDomain: Connection Killed. host {host}:{port}. clientID:{client.Key}");
            client.Dispose();
            CheckDispose();
            client.ReportTime($"ConnectionsToDomain: IsDisposed:{isDisposed}");
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

        public void Dispose()
        {
            Console.WriteLine($"ConnectionToDomain {host}:{port} disposed");
            if (isDisposed) throw new ObjectDisposedException($"Object is already disposed");
            isDisposed = true;
            startTime = default;
            checkTime = default;
            host = default;
            port = default;
            pool!.Return(this);
            pool = default;
            counter = default;
            //timeoutDefault = default;
            timeoutSpecified = default;

            if (head != null)
            {
                throw new NotImplementedException($"Connections must be properly closed");
            }
            Key = default;
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

        public void Start()
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
                    this.returnToManager(this);
                }
            }
        }

        public void RegistRuturnToManager(Action<ConnectionsToDomain<T1>> returnToManager)
        {
            this.returnToManager = returnToManager;
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