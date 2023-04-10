using HttpDecodingProxy.http;
using IziHardGames.Libs.NonEngine.Memory;
using ProxyLibs.Extensions;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace IziHardGames.Proxy.Sniffing.Http
{
    /// <summary>
    /// One connection to origin from many agents acepted from browser
    /// </summary>
    public class HttpClientForIntercepting : IDisposable
    {
        public string AddressAndPort { get; set; }

        #region Networking
        public Stream streamToOrigin;
        protected TcpClient tcpToOrigin;
        public List<HttpAgentConnection> agents = new List<HttpAgentConnection>();

        #endregion

        protected readonly ConnectionConfig config = new ConnectionConfig();

        public ManagerForHttpMessages managerForMessages;
        public ManagerForHttpAgentConnection managerForHttpAgentConnection;

        private readonly ConcurrentQueue<HttpProxyMessage> messageQueues = new ConcurrentQueue<HttpProxyMessage>();

        public Task RunTask;
        protected readonly CancellationTokenSource cts = new CancellationTokenSource();

        // Events
        public Action<HttpRequest> ActionModifyRequest;
        public Action<HttpResponse> ActionModifyResponse;
        public Action<HttpRequest> OnActionRequest;
        public Action<HttpResponse> OnActionResponse;
        public static readonly Action<HttpRequest> DummyRequest;
        public static readonly Action<HttpResponse> DummyResponse;

        protected bool IsStarted;
        protected bool IsConnecteToOrigin;

        protected int CountMsg { get; set; }

        static HttpClientForIntercepting()
        {
            DummyRequest = (x) => { };
            DummyResponse = (x) => { };
        }

        protected virtual void Execute(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (IsConnecteToOrigin)
                {
                    if (tcpToOrigin.Connected)
                    {
                        if (messageQueues.TryDequeue(out HttpProxyMessage msg))
                        {
                            Logger.LogLine($"Begin redirect msg:{Environment.NewLine}{msg.request.fields.ToStringInfo()}");

                            var agent = msg.agent;

                            if (config.max > 0)
                            {
                                CountMsg++;
                                if (CountMsg >= config.max) throw new Exception("Max Msg Count Reached");
                            }

                            Task.Run(() => OnActionRequest(msg.request.DeepCopy()));
                            // modify
                            ActionModifyRequest(msg.request);
                            // send
                            msg.request.Write(streamToOrigin);

                            msg.ReadMsgInto(streamToOrigin, agent.StreamRaw, msg.response);

                            Task.Run(() => OnActionResponse(msg.response.DeepCopy()));

                            ActionModifyResponse(msg.response);

                            msg.response.Write(agent.Stream);

                            Logger.LogLine($"Msg Complete:{msg.response.fields.ToStringInfo()}", ConsoleColor.Green);

                            agent.SetAwaitResponse(false);

                            msg.Dispose();
                        }
                    }

                }
            }
            Dispose();
        }

        private void Reconnect()
        {
            throw new NotImplementedException();
        }

        public HttpResponse EnqueueMsg(HttpProxyMessage msg)
        {
            messageQueues.Enqueue(msg);
            return msg.response;
        }

        protected void Tunneling(Stream client, Stream server, CancellationToken ct)
        {
            Task.Run(() => CopyStream(client, server), ct);
            Task.Run(() => CopyStream(server, client), ct);
        }

        public void StartOrUpdateDequeueMsgMode(HttpProxyMessage msg, ManagerForHttpAgentConnection managerForConnections, ManagerForHttpMessages managerForHttpMessages)
        {
            lock (this)
            {
                this.managerForHttpAgentConnection = managerForConnections;
                this.managerForMessages = managerForHttpMessages;

                if (!IsStarted)
                {   /// <see cref="HttpClientForInterceptingSsl.Connect(HttpRequest)"/>
                    MakeInterceptingBridge(msg.request, msg.agent);
                    StartDequeueMsgMode();
                    SetSettingsFromRequest(msg);
                }
                else
                {
                    UpdateSettingsFromRequest(msg);
                    //throw new System.NotImplementedException("Need to resolve simultenious CONNECT method");
                }
            }
        }
        protected virtual void StartDequeueMsgMode()
        {
            Start();
            RunTask = Task.Run(() => Execute(cts.Token));
        }

        protected void Start()
        {
            ActionModifyRequest = DummyRequest;
            ActionModifyResponse = DummyResponse;

            OnActionRequest = DummyRequest;
            OnActionResponse = DummyResponse;
            IsStarted = true;
        }
        protected virtual void SetSettingsFromRequest(HttpProxyMessage msg)
        {
            if (msg.request.fields.TryFindTearDown(out string value))
            {
                //The "close" connection option is defined as a signal that the sender will close this connection after completion of the response.
            }
        }
        protected virtual void UpdateSettingsFromRequest(HttpProxyMessage msg)
        {
            config.UpdateFromRequest(msg);

            CountMsg = config.max;

            if (streamToOrigin != null)
            {
                if (config.timeout > 0)
                {
                    streamToOrigin.WriteTimeout = config.timeout;
                    streamToOrigin.ReadTimeout = config.timeout;
                }
            }

            var agent = msg.agent;
            var streamToAgent = agent.Stream;

            if (streamToAgent != null)
            {
                if (config.timeout > 0)
                {
                    streamToAgent.WriteTimeout = config.timeout;
                    streamToAgent.ReadTimeout = config.timeout;
                }
            }
        }


        protected virtual void MakeInterceptingBridge(HttpRequest request, HttpAgentConnection agent)
        {
            ConnectToOrigin(request.fields.HostAddress, request.fields.HostPort);
            streamToOrigin = tcpToOrigin.GetStream();
            IsConnecteToOrigin = true;
            agents.Add(agent);
        }
        protected void ConnectToOrigin(string hostname, int port)
        {
            TcpClient toOrigin = new TcpClient(hostname, port);
            var streamToOrigin = toOrigin.GetStream();
            tcpToOrigin = toOrigin;
        }

        public void Terminate()
        {
            throw new Exception($"Terminated");
            cts.Cancel();
        }
        public virtual void Dispose()
        {
            Logger.LogLine($"Disposed: {this}");

            if (tcpToOrigin != null)
            {
                tcpToOrigin.Dispose();
            }

            tcpToOrigin = default;
            CountMsg = default;
            AddressAndPort = default;
            RunTask = default;
            IsConnecteToOrigin = false;
            managerForMessages = default;
            managerForHttpAgentConnection = default;

            cts.TryReset();
            if (GetType() == typeof(HttpClientForIntercepting))
            {
                PoolObjects<HttpClientForIntercepting>.Shared.Return(this);
            }
        }
        public static void CopyStream(Stream from, Stream to)
        {
            byte[] bytes = new byte[8096];

            try
            {
                while (true)
                {
                    var readed = from.Read(bytes, 0, bytes.Length);
                    to.Write(bytes, 0, readed);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}