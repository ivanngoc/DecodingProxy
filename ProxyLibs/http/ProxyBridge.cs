using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp;
using IziHardGames.Proxy.Tcp.Tls;
using ProxyLibs.Extensions;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Sockets;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{

    /// <summary>
    /// Intermediary between connection to origin and connection to agent
    /// </summary>
    /// One connection to origin from many agents acepted from browser???
    public class ProxyBridge : IDisposable
    {
        #region Networking
        public string AddressAndPort => connectionsToDomain.AddressAndPort;
        public ConnectionToAgentTls Cta => connectionToAgent;
        public ConnectionToOriginTls Cto => connectionToOrigin;

        protected ConnectionToAgentTls connectionToAgent;
        protected ConnectionsToDomainTls connectionsToDomain;
        protected ConnectionToOriginTls connectionToOrigin;
        #endregion

        private EClientStatus status;
        public StartOptions startOptions;

        protected readonly ConnectionConfig config = new ConnectionConfig();

        public ManagerForHttpMessages managerForMessages;

        public Task RunTask;
        public TcpWrap wrap;

        // Events
        public Action<HttpRequest> ActionModifyRequest;
        public Action<HttpResponse> ActionModifyResponse;
        public Action<HttpRequest> OnActionRequest;
        public Action<HttpResponse> OnActionResponse;

        public static readonly Action<HttpRequest> DummyRequest;
        public static readonly Action<HttpResponse> DummyResponse;
        protected bool IsConnected => Cta.Connected && Cto.Connected;
        protected int CountMsg { get; set; }
        public readonly int id;

        static ProxyBridge()
        {
            DummyRequest = (x) => { };
            DummyResponse = (x) => { };
        }

        public ProxyBridge()
        {
            id = GetHashCode();
        }

        public virtual void InitToReuse(ConnectionsToDomainTls ctd, StartOptions startOptions)
        {
            this.connectionsToDomain = ctd;
            this.startOptions = startOptions;
        }
        public virtual async Task MakeInterceptingBridge(ConnectionToOriginTls cto, ConnectionToAgentTls cta)
        {
            Bind(cto);
            Bind(cta);
        }

        #region Message Mode

        protected void Execute()
        {
            var streamToOrigin = connectionToOrigin.Stream;
            var agent = this.connectionToAgent;
            var agentStream = agent.SslStream;
            this.status = EClientStatus.Initilized;

            while (IsConnected)
            {
                var msg = managerForMessages.GetNew();
                this.status = EClientStatus.AwaitRequestFromAgent;
                try
                {
                    if (HttpProxyMessage.TryReadMsgInto(agentStream, msg.request))
                    {
                        this.status = EClientStatus.SendingRequestToOrigin;

                        Logger.LogLine($"[{id}] Begin redirect msg:{Environment.NewLine}{msg.request.fields.ToStringInfo()}", ConsoleColor.DarkGreen);

                        if (config.max > 0)
                        {
                            CountMsg++;
                            if (CountMsg >= config.max) throw new Exception("Max Msg Count Reached");
                        }

                        //Task.Run(() => OnActionRequest(msg.request.DeepCopy()));
                        // modify
                        ActionModifyRequest(msg.request);

                        if (Cto.Ctd.isSslProbed)
                        {
                            // send
                            msg.request.WriteTo(streamToOrigin);
                            this.status = EClientStatus.ReadingResponseFromOrigin;
                            Logger.LogLine($"[{id}] Begin Read Response");
                            HttpProxyMessage.ReadMsgInto(streamToOrigin, msg.response);
                            this.status = EClientStatus.CopyResponseToAgent;
                            Logger.LogLine($"[{id}] Response readed:{Environment.NewLine}{msg.response.fields.ToStringInfo()}");


                            //Task.Run(() => OnActionResponse(msg.response.DeepCopy()));

                            ActionModifyResponse(msg.response);

                            msg.response.WriteTo(agentStream);
                            this.status = EClientStatus.Complete;

                            Logger.LogLine($"[{id}] Msg Complete:{Environment.NewLine}{msg.response.fields.ToStringInfo()}", ConsoleColor.Green);
                        }
                        else
                        {
                            agentStream.Write(HttpProxyMessage.response404);
                        }

                        bool isCloseAgent = msg.request.fields.CheckCloseConnection();
                        bool isCloseOrigin = msg.response.fields.CheckCloseConnection();

                        msg.Dispose();

                        if (isCloseAgent || isCloseOrigin)
                        {
                            Logger.LogLine($"[{id}] Close Connection is demanded for {AddressAndPort}");
                            break;
                        }
                    }
                }
                catch (IOException ex)
                {
                    Logger.LogException(ex);
                    msg.Dispose();
                    break;
                }
            }
        }

        public void StartDequeueMsgMode(StartOptions options, ManagerForHttpMessages managerForHttpMessages, TcpWrap wrap)
        {
            Logger.LogLine($"{nameof(StartDequeueMsgMode)} [{id}]");

            this.managerForMessages = managerForHttpMessages;

            /// <see cref="ProxyBridgeSsl.Connect(HttpRequest)"/>
            Start(wrap);
            Execute();
        }
        #endregion

        #region Two Way Non Blocking Mode
        public async Task RunTwoWayMode(StartOptions options, TcpWrap wrap, IBlockConsumer[] consumersRequest, IBlockConsumer[] consumersResponse)
        {
            Start(wrap);

            using (TwoWayMode twoWayMode = PoolObjectsConcurent<TwoWayMode>.Shared.Rent())
            {
                await twoWayMode.Run(Cta.Stream, Cto.Stream, consumersRequest, consumersResponse, options).ConfigureAwait(false);
                PoolObjectsConcurent<TwoWayMode>.Shared.Return(twoWayMode);
            }
        }

        #endregion

        protected void Start(TcpWrap wrap)
        {
            this.wrap = wrap;
            ActionModifyRequest = DummyRequest;
            ActionModifyResponse = DummyResponse;

            OnActionRequest = DummyRequest;
            OnActionResponse = DummyResponse;
        }
        public virtual void Dispose()
        {
            Logger.LogLine($"Disposed [{id}]: {AddressAndPort}", ConsoleColor.Red);

            connectionToAgent.Dispose();
            connectionToOrigin.Dispose();
            connectionsToDomain.Disconnect(this);

            CountMsg = default;
            RunTask = default;
            managerForMessages = default;

            connectionsToDomain = default;
            connectionToOrigin = default;
            connectionToAgent = default;

            this.status = EClientStatus.None;

            if (GetType() == typeof(ProxyBridge))
            {
                PoolObjects<ProxyBridge>.Shared.Return(this);
            }
        }
        protected void Bind(ConnectionToOriginTls cto)
        {
            this.connectionToOrigin = cto;
        }
        protected void Bind(ConnectionToAgentTls cta)
        {
            this.connectionToAgent = cta;
        }
    }
}