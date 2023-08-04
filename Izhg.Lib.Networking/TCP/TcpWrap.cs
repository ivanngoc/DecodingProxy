using IziHardGames.Core;
using IziHardGames.Libs.Networking.Clients;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.Tcp
{

    public class TcpWrap : IKey<uint>, IPerfTracker, IPoolBind<TcpWrap>, IGuid, IDisposable, IClient, IInitializable<string>, IConnectorTcp, IConnection, IConnectionData
    {
        public TcpClient Client { get; set; }
        public uint Key { get; set; }
        public Guid Guid { get => guid; }
        public EConnectionState State => state;
        public EConnectionFlags Flags => flags;
        public int Available { get; set; }
        public string Title { get; set; }
        public string Host { get => host; set => host = value; }
        public int Port { get => port; set => port = value; }
        public int Id { get => (int)Key; set => throw new System.NotImplementedException(); }
        public int Action { get; set; }
        public string Version { get => $"GEN:{generation}. Deaths:{deaths}. Title:{Title}. Protocol:TCP"; set => throw new System.NotImplementedException(); }
        public string Status { get => $"Sate:{state.ToString()} flags:{FlagsToInfo()}"; set => throw new System.NotImplementedException(); }

        protected string host;
        protected int port;
        protected int life;
        protected EConnectionState state = EConnectionState.None;
        protected EConnectionFlags flags = EConnectionFlags.None;
        private readonly Guid guid;
        protected NetworkStream stream;

        private bool isDisposed = true;
        private int generation;
        private int deaths;
        private IPoolReturn<TcpWrap> pool;
#if DEBUG
        private List<string> logs = new List<string>();
        private List<object> objs = new List<object>();
#endif
        public TcpWrap()
        {
            guid = Guid.NewGuid();
        }

        public void Wrap(TcpClient tcpClient)
        {
            Client = tcpClient;
            stream = tcpClient.GetStream();
        }
        public void Initilize(string title)
        {
            this.Title = title;
            Initilize();
        }

        protected void Initilize()
        {
            generation++;
            if (!isDisposed) throw new ObjectDisposedException("Object must be disposed before reuse");
            isDisposed = false;
        }

        public virtual async Task ConnectAsyncTcp(string host, int port)
        {
            this.host = host;
            this.port = port;
            var client = new TcpClient();
            try
            {
                await client.ConnectAsync(host, port);
            }
            catch (SocketException ex)
            {
                throw;
            }
            Wrap(client);
        }
        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("Object already disposed");
            isDisposed = true;

            deaths++;
#if DEBUG
            Console.WriteLine($"Disposed {Guid}");
            logs.Clear();
            objs.Clear();
#endif
            Client.Dispose();
            Client = default;

            if (pool != null) pool.Return(this);
            state = EConnectionState.Disposed;
            flags = EConnectionFlags.Reseted;
            pool = default;
            Key = default;
            Title = default;
            life = default;
            stream = default;
        }

        public void BindToPool(IPoolReturn<TcpWrap> pool)
        {
            this.pool = pool;
        }

        public void ReportTime(string log)
        {
            var text = $"{DateTime.Now.ToString("HH:mm:ss.ffffff")} GEN:{generation}. Deaths:{deaths}. GUID:{Guid}. Title:{Title}.  [{GetType().Name}{nameof(ReportTime)}()] {log}.";
            logs.Add(text);
#if DEBUG
            //Console.WriteLine(text);
#endif
        }

        public void PutMsg<T>(T o) where T : ICloneable
        {
            objs.Add(o);
        }

        public void ConsumeLife()
        {
            life--;
        }

        public virtual async Task SendAsync(byte[] mem, CancellationToken token = default)
        {
            await Client.GetStream().WriteAsync(mem, 0, mem.Length, token).ConfigureAwait(false);
        }
        public virtual async Task SendAsync(ReadOnlyMemory<byte> mem, CancellationToken token)
        {
            await Client.GetStream().WriteAsync(mem, token).ConfigureAwait(false);
        }

        public Task RunWriterLoop()
        {
            return Task.CompletedTask;
        }

        public Task StopWriteLoop()
        {
            return Task.CompletedTask;
        }

        public void SetTimeouts(int send, int recieve)
        {
            Client.SendTimeout = send;
            Client.ReceiveTimeout = recieve;
        }

        public void SetLife(int max)
        {
            life = max;
        }

        public bool CheckConnectIndirectly()
        {
            return CheckConnect();
        }

        public bool CheckConnect()
        {
            if (isDisposed) throw new ObjectDisposedException("Object disposed");
            return Client.Connected;
        }
        public virtual bool CheckData()
        {
            return stream.DataAvailable;
        }

        public void SetState(EConnectionState state)
        {
            this.state = state;
        }
        public string ToInfoConnectionData()
        {
            return $"ConnectionData. GetType():{GetType().FullName}; host:{Host}; port:{Port}; id:{Id}; action:{Action}; status:{Status}; version:{Version}";
        }

        public string FlagsToInfo()
        {
            if (flags == EConnectionFlags.Reseted)
            {
                return $"Reseted";
            }

            string result = string.Empty;
            if (flags.HasFlag(EConnectionFlags.AuthenticatedSslClient))
            {
                result += "Authenticated; ";
            }
            if (flags.HasFlag(EConnectionFlags.LifePresented))
            {
                result += $"Life is Set:{life}; ";
            }
            if (flags.HasFlag(EConnectionFlags.TimeoutPresented))
            {
                result += $"Recieve Timeout is Set:{Client.ReceiveTimeout}; ";
            }
            return result;
        }
    }
}