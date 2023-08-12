using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Core;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp;
using Microsoft.Extensions.Logging;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public class SocketWrap : IKey<uint>, IPoolBind<SocketWrap>, IGuid, IDisposable, IClient<SocketReader, SocketWriter>, IInitializable<string>, IConnectorTcp, IConnection<ConnectionDataPoolable>, IGetLogger, IUpgradableConnection<SocketWrapUpgrade>
    {
        public Socket Socket { get; set; }
        public uint Key { get; set; }
        public Guid Guid { get => guid; }
        public EConnectionState State => state;
        public EConnectionFlags Flags => flags;
        public int Available { get; set; }
        /// <summary>
        /// Custom defined name
        /// </summary>
        public string Title { get; set; }
        public string Version { get => $"GEN:{generation}. Deaths:{deaths}. Title:{Title}. Protocol:TCP"; set => throw new NotImplementedException(); }
        public string Status { get => $"Sate:{state.ToString()} flags:{FlagsToInfo()}"; set => throw new NotImplementedException(); }
        public ILogger Logger => logger;
        public ConnectionDataPoolable ConnectionData => connectionData;
        public SocketReader Reader => reader;
        public SocketWriter Writer => writer;
        public string InfoPrefix => $"{DateTime.Now.ToString("HH:mm:ss.ffffff")} GEN:{generation}. Deaths:{deaths}. GUID:{Guid}. Title:{Title}. ";

        protected int life;
        protected EConnectionState state = EConnectionState.None;
        protected EConnectionFlags flags = EConnectionFlags.None;
        private readonly Guid guid;

        private bool isDisposed = true;
        private int generation;
        private int deaths;
        private IPoolReturn<SocketWrap> pool;
        /// <summary>
        /// Transport protocol upgrade
        /// </summary>
        public readonly List<SocketWrapUpgrade> upgrades = new List<SocketWrapUpgrade>();
        public readonly Dictionary<Type, SocketWrapModifier> modifiers = new Dictionary<Type, SocketWrapModifier>();

        public readonly SocketWrapLogger logger;

        private SocketReader reader;
        private SocketWriter writer;
        private ConnectionDataPoolable connectionData;

        public SocketWrap()
        {
            guid = Guid.NewGuid();
            logger = new SocketWrapLogger(this);
        }

        public void Wrap(Socket socket, bool isDefaultReader = true, bool isDefaultWriter = true)
        {
            Socket = socket;
            if (isDefaultReader) reader = SocketReaderDefault.Rent();
            if (isDefaultWriter) writer = SocketWriterDefault.Rent();
        }
        public void Initilize(string title)
        {
            Title = title;
            Initilize();
        }

        protected void Initilize()
        {
            generation++;
            if (!isDisposed) throw new ObjectDisposedException("Object must be disposed before reuse");
            isDisposed = false;
        }

        public async Task ConnectAsyncTcp(string host, int port)
        {
            connectionData.Host = host;
            connectionData.Port = port;
            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await socket.ConnectAsync(host, port);
            }
            catch (SocketException ex)
            {
                throw;
            }
            Wrap(socket, false, false);
        }
        public void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("Object already disposed");
            isDisposed = true;

            deaths++;
#if DEBUG
            Console.WriteLine($"Disposed {Guid}");
#endif
            Socket.Dispose();
            Socket = default;

            if (pool != null) pool.Return(this);
            state = EConnectionState.Disposed;
            flags = EConnectionFlags.Reseted;
            pool = default;
            Key = default;
            Title = default;
            life = default;

            foreach (var upgrade in upgrades)
            {
                upgrade.Dispose();
            }
            upgrades.Clear();

            foreach (var item in modifiers.Values)
            {
                item.Dispose();
            }
            modifiers.Clear();
        }

        public void BindToPool(IPoolReturn<SocketWrap> pool)
        {
            this.pool = pool;
        }
        public void ConsumeLife()
        {
            life--;
        }

        public void SetTimeouts(int send, int recieve)
        {
            Socket.SendTimeout = send;
            Socket.ReceiveTimeout = recieve;
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
            return Socket.Connected;
        }
        public bool CheckData()
        {
            return Socket.Available > 0;
        }

        public void SetState(EConnectionState state)
        {
            this.state = state;
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
                result += $"Recieve Timeout is Set:{Socket.ReceiveTimeout}; ";
            }
            return result;
        }

        public void DisableFlags(EConnectionFlags flags)
        {
            this.flags &= ~flags;
        }
        public void EnableFlags(EConnectionFlags flags)
        {
            this.flags |= flags;
        }

        private async Task SendV0(byte[] mem, CancellationToken token = default)
        {
            await Socket.SendAsync(mem.AsMemory(0, mem.Length), SocketFlags.None, token).ConfigureAwait(false);
        }

        public void AddUpgrade<T>(T upgrade) where T : SocketWrapUpgrade
        {
            upgrades.Add(upgrade);
            upgrade.ApplyTo(this);
        }
        public Task SendAsync(ReadOnlyMemory<byte> mem, CancellationToken token)
        {
            throw new NotImplementedException();
        }
        public void AddModifier<T>(IPoolObjects<T> pool) where T : SocketWrapModifier
        {
            var mod = pool.Rent();
            if (mod is IPoolBind<T> bindable) bindable.BindToPool(pool);
            mod.Initilize(this);
        }

        public void SetWriter(SocketWriter writer)
        {
            this.writer = writer;
        }
        public void SetReader(SocketReader reader)
        {
            this.reader = reader;
        }
        public SocketWrapUpgrade UpgradeTls(SslClientAuthenticationOptions options)
        {
            throw new NotImplementedException();
        }
    }
}