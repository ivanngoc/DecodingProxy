using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Sniffing.ForHttp;
using IziHardGames.Proxy.Tcp;
using IziHardGames.Proxy.Tcp.Tls;
using IziHardGames.Proxy.TcpDecoder;
using IziHardGames.Tls;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ConnectionsToDomain = IziHardGames.Proxy.Tcp.ConnectionsToDomain<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>;
using ManagerConnectionsToDomain = IziHardGames.Libs.ObjectsManagment.ManagerBase<string, IziHardGames.Proxy.Tcp.ConnectionsToDomain<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>>;
using ManagerConnectionsToDomainSsl = IziHardGames.Libs.ObjectsManagment.ManagerBase<string, IziHardGames.Proxy.Tcp.Tls.ConnectionsToDomainTls>;

namespace IziHardGames.Proxy.Http
{
    public class HttpSpyProxy : IProxy, IDisposable
    {
        private X509Certificate2 caRootCert;
        public X509Certificate2 CaRootCert => caRootCert;
        private readonly TcpServer tcpServer = new TcpServer();

        public GlobalProxySettings settings = new GlobalProxySettings();
        public readonly ManagerForHttpClientForIntercepting managerForHttpClientForIntercepting = new ManagerForHttpClientForIntercepting();
        public readonly ManagerForHttpMessages managerForHttpMessages = new ManagerForHttpMessages();
        public readonly ManagerForConnectionToAgent managerForConnectionsToAgent = new ManagerForConnectionToAgent();
        public readonly ManagerConnectionsToDomain manager;
        public readonly ManagerConnectionsToDomainSsl managerSsl;
        public readonly ManagerForConnectionsToDomain managerForConnectionsToDomain = new ManagerForConnectionsToDomain();

        private object lockList = new object();
        public CertManager certManager;
        private static List<string> sniffList = new List<string>();
        public ulong ticks;
        private ConsumingProvider consumingProvider;
        private IBlockConsumer[] consumersRequest;
        private IBlockConsumer[] consumersResponse;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ILogger logger;

        public HttpSpyProxy()
        {
            manager = new ManagerConnectionsToDomain((key) =>
            {
                var pool = PoolObjectsConcurent<ConnectionsToDomain>.Shared;
                var rent = pool.Rent();
                rent.BindToPool(pool);
                rent.Key = key;
                rent.Start();
                rent.RegistRuturnToManager(manager!.Return);
                return rent;
            });

            managerSsl = new ManagerConnectionsToDomainSsl((key) =>
            {
                var pool = PoolObjectsConcurent<ConnectionsToDomainTls>.Shared;
                var rent = pool.Rent();
                rent.Key = key;
                rent.BindToPool(pool);
                rent.RegistRuturnToManager(managerSsl!.Return);
                rent.Start();
                return rent;
            });
        }

        private void Init(ILogger logger, CertManager certManager, ConsumingProvider consumingProvider)
        {
            Console.WriteLine($"это на русском языке");
            this.logger = logger;
            this.certManager = certManager;
            this.consumersRequest = consumingProvider.consumersRequest;
            this.consumersResponse = consumingProvider.consumersResponse;

            managerForHttpClientForIntercepting.Init(managerForConnectionsToDomain, managerForConnectionsToAgent, managerForHttpMessages);
        }

        public async Task Run(CancellationToken token)
        {
            try
            {
                caRootCert = CertManager.LoadPemFromFile(ConfigJson.path_ca_cert, ConfigJson.path_ca_key);
            }
            catch (Exception ex)
            {
                Stop();
                Logger.LogException(ex);
                throw ex;
            }
            //tcpServer.OnClientConnectEvent += AcceptClient;
            //tcpServer.OnClientConnectSslEvent += AcceptClientSsl;

            Settings.IsLogToFile = false;

            DecodingProxyServerConfig.Singleton = new DecodingProxyServerConfig();

            foreach (var host in DecodingProxyServerConfig.Singleton.HostsForSniffing)
            {
                Logger.SetFilter(host);
            }

            //tcpServer.Start(token);
            var task = Task.Run(async () => await RunWithPipedTcp(token), token);
            var taskSsl = Task.Run(async () => await tcpServer.StartSSL(AcceptClientSsl, token), token);
            await Task.WhenAll(task, taskSsl).ConfigureAwait(false);
            //await task.ConfigureAwait(false);
        }

        private async Task RunWithPipedTcp(CancellationToken token)
        {
            using (TcpServerPiped pipedTcpServer = new TcpServerPiped())
            {
                await pipedTcpServer.Run("localhost", TcpServer.port, logger, AcceptClient, token).ConfigureAwait(false);
            }
        }
        private async Task<TcpClientPiped> AcceptClient(TcpClientPiped agent, CancellationToken token)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            var pool = PoolObjectsConcurent<HttpPipedIntermediary>.Shared;
            using (HttpPipedIntermediary parserRequest = pool.Rent().Init(consumingProvider, manager, pool))
            {
                await parserRequest.Run(agent, cts).ConfigureAwait(false);
            }
            Console.WriteLine($"{nameof(HttpSpyProxy)} AcceptClient loop ended");
            return agent;
        }

        private async Task<TcpWrap> AcceptClientSsl(TcpWrap obj)
        {
            Logger.LogLine($"HTTPS Client Accepted");

            var task = Task.Run(async () =>
                    {
                        await AcceptWithTwoWayModeSsl(obj).ConfigureAwait(false);
                    }, cts.Token);

            MonitorForTasks.Watch(task);

            await Task.WhenAll(task);
            return obj;
        }

        [Obsolete]
        private async Task<ProxyBridge> CreateBridge(TcpWrap tcpDecodingClient)
        {
            var client = tcpDecodingClient.Client;
            var stream = client.GetStream();

            var initMsg = managerForHttpMessages.ReadFirstRequest(stream);
            var startOptions = initMsg.ToStartOptions();
            startOptions.cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
            startOptions.consumingProvider = consumingProvider;

            SelectProtocol(initMsg);

            string host = startOptions.Host;
            Logger.LogLine($"Init Msg:{host}. Method:{initMsg.request.fields.Method}{Environment.NewLine}{initMsg.request.fields.ToStringInfo()}", ConsoleColor.Yellow);

            initMsg.Dispose();

            ConnectionsToDomainTls ctd = managerForConnectionsToDomain.GetOrCreate(startOptions, caRootCert);
            ProxyBridge proxyBridge = await managerForHttpClientForIntercepting.CreateBridge(this, ctd, tcpDecodingClient, startOptions).ConfigureAwait(false);

            Logger.LogLine($"Connection to {host} allocated");
            return proxyBridge;
        }
        private async Task MaintainSslSession(TcpClientPiped tcpClientPiped, CancellationTokenSource cts)
        {




            //var client = tcpDecodingClient.Client;
            //var stream = client.GetStream();

            //var initMsg = managerForHttpMessages.ReadFirstRequest(stream);
            //var startOptions = initMsg.ToStartOptions();
            //startOptions.cts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token);
            //startOptions.consumingProvider = consumingProvider;

            //SelectProtocol(initMsg);

            //string host = startOptions.Host;
            //Logger.LogLine($"Init Msg:{host}. Method:{initMsg.request.fields.Method}{Environment.NewLine}{initMsg.request.fields.ToStringInfo()}", ConsoleColor.Yellow);

            //initMsg.Dispose();

            //ConnectionsToDomainTls ctd = managerForConnectionsToDomain.GetOrCreate(startOptions, caRootCert);
            //ProxyBridge proxyBridge = await managerForHttpClientForIntercepting.CreateBridge(this, ctd, tcpDecodingClient, startOptions).ConfigureAwait(false);

            //Logger.LogLine($"Connection to {host} allocated");
            //return proxyBridge;


        }

        private void SelectProtocol(HttpProxyMessage initMsg)
        {
            var version = initMsg.request.fields.Version;
            if (version != HttpLibConstants.version11.ToLowerInvariant()) throw new NotSupportedException($"Protocol Other Than HTTP/1.1 is not implemented yet");
        }
        [Obsolete]
        private async Task AcceptWithTwoWayModeSsl(TcpWrap wrap)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(this.cts.Token);
            var ns = wrap.Client.GetStream();
            ns.ReadTimeout = 15000;

            var bridge = await CreateBridge(wrap).ConfigureAwait(false);
            StartOptions options = bridge.startOptions;

            if (options.ValidateHttps())
            {
                await bridge.RunTwoWayMode(options, wrap, consumersRequest, consumersResponse).ConfigureAwait(false);
                managerForHttpClientForIntercepting.Remove(bridge);
                CloseConnection(bridge);
                Logger.LogLine($"Connection closed ssl", ConsoleColor.Cyan);
            }
            else
            {
                Stop();
                throw new NotImplementedException();
            }
            options.Dispose();
        }
        private async Task AcceptPipedSsl(TcpClientPiped agent)
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(this.cts.Token);

            var pool = PoolObjectsConcurent<HttpPipedIntermediary>.Shared;
            var pool2 = PoolObjectsConcurent<TcpClientPipedSsl>.Shared;

            var taskFill = agent.StopWriteLoop();

            using (HttpPipedIntermediary parserRequest = pool.Rent().Init(consumingProvider, manager, pool))
            {
                using (var initMsg = await parserRequest.AwaitMsg(HttpLibConstants.TYPE_REQUEST, agent, cts, PoolObjectsConcurent<HttpBinary>.Shared).ConfigureAwait(false))
                {
                    if (initMsg.FindMethod() == EHttpMethod.CONNECT)
                    {
                        var pair = initMsg.FindHostAndPortFromField();
                        var hub = managerSsl.GetOrCreate($"{pair.Item1}:{pair.Item2}");
                        hub.UpdateAddress(pair.Item1, pair.Item2);
                        var t1 = hub.GetOrCreate("Origin Ssl", pool2);
                        var t2 = agent.SendAsync(HttpProxyMessage.response200, cts.Token);

                        SslStream agentSsl = new SslStream(agent);

                        await t1.ConfigureAwait(false);
                        await t2.ConfigureAwait(false);
                        //await Task.WhenAll(t1, t2).ConfigureAwait(false);

                        var origin = t1.Result;

                        SslStream originSsl = new SslStream(agent);

                    }
                    else
                    {
                        // need maintain connection without CONNECT
                        throw new NotImplementedException($"On accepting client with SSL port there must be CONNECT method first if this app behave as proxy (NOT MITM)");
                    }
                    await MaintainSslSession(agent, cts).ConfigureAwait(false);
                }
            }
            await Task.WhenAll(taskFill).ConfigureAwait(false);

            Console.WriteLine($"{nameof(HttpSpyProxy)} AcceptClient loop ended");
        }

        private void CloseConnection(ProxyBridge item)
        {
            item.wrap.Close();
            item.Dispose();
        }

        private void StartTunnelMode(TcpWrap tcpDecodingClient)
        {
            InterceptorHttp interceptor = new InterceptorHttp(tcpDecodingClient, managerForHttpClientForIntercepting, managerForHttpMessages, this);
            MonitorForTasks.Watch(Task.Run(interceptor.Start));
        }
        public void Dispose()
        {
            if (!cts.TryReset())
            {
                cts = new CancellationTokenSource();
            }
            sniffList.Clear();
        }

        private static void Sniff(string hostname)
        {
            if (sniffList.Contains(hostname)) return;
            sniffList.Add(hostname);
        }
        public static bool IsSniffTarget(string hostname)
        {
            return sniffList.Contains(hostname);
        }

        public async Task Run(ILogger logger, ConsumingProvider consumingProvider)
        {
            Logger.LogLine($"STart Proxy decoding");
            this.consumingProvider = consumingProvider;
            ConfigJson.EnsureConfigExist();
            UpdateSettings();
            CertManager.CreateDefault(ConfigJson.PathCertForged, ConfigJson.PathCertOriginal);
            Init(logger, CertManager.Shared, consumingProvider);
            await Run(cts.Token).ConfigureAwait(false);
        }

        public async Task Stop()
        {
            cts.Cancel();
            Logger.LogException(new ApplicationException($"{nameof(HttpSpyProxy)} Has Stopped"));
        }

        private static void UpdateSettings()
        {
            ConfigJson.Init();
            UpdateSettings(ConfigJson.configJson);
        }

        private static void UpdateSettings(string json)
        {
            var rules = ConfigJson.Rules;

            foreach (var rule in rules)
            {
                string domain = (string)rule["domain"];
                var optionsNode = rule["options"];
                int[] options = optionsNode.Deserialize<int[]>();
                Sniff(domain);
            }
        }

        public static void Test()
        {
            TcpListener tcpListener = new TcpListener(TcpServer.DEFAULT_PORT_SSL);
            tcpListener.Start();

            while (true)
            {
                var agent = tcpListener.AcceptTcpClient();
                TcpWrap tdc = new TcpWrap(agent);

                HttpProxyMessage msg = new HttpProxyMessage();
                var streamToAgent = agent.GetStream();
                HttpProxyMessage.ReadMsgInto(streamToAgent, msg.request);
                var options = msg.ToStartOptions();

                Logger.LogLine(msg.request.sb.ToString());
                Logger.LogLine("------------------------------------------------");

                if (!msg.request.fields.InitLine.Contains("google") && !msg.request.fields.InitLine.Contains("CONNECT:"))
                {
                    agent.Close();
                    continue;
                }

                if (msg.request.fields.IsConnectRequired)
                {
                    Logger.LogLine("Connection required pass");

                    /// https://httpwg.org/specs/rfc9112.html#message.body.length<br/>
                    /// Any 2xx (Successful) response to a CONNECT request implies that the connection will become 
                    /// a tunnel immediately after the empty line that concludes the header fields. 
                    /// A client MUST ignore any Content-Length or Transfer-Encoding header fields received in such a message.
                    streamToAgent.Write(HttpProxyMessage.response200);
                    Logger.LogLine("Sended 200 Ok to client");

                    TcpClient origin = new TcpClient();
                    origin.Connect(msg.request.fields.HostAddress, msg.request.fields.HostPort);
                    Logger.LogLine("Connected to Origin");

                    /// do not access to <see cref="Socket.Available"/> after creating <see cref="SslStream"/>. Somehow it closing connection
                    Stream streamToOrigin = origin.GetStream();
                    SslStream sslToServer = new SslStream(streamToOrigin);
                    sslToServer.AuthenticateAsClient(msg.request.fields.HostAddress);
                    Logger.LogLine("Authenticated as client");

                    var serverCert = sslToServer.RemoteCertificate;
                    var caCert = CertManager.LoadPemFromFile(ConfigJson.path_ca_cert, ConfigJson.path_ca_key);
                    var cert = CertManager.Shared.ForgedGetOrCreateCertFromCache((X509Certificate2)serverCert, caCert);
                    Logger.LogLine("Forged SSL");

                    SslStream sslToAgent = new SslStream(streamToAgent);
                    sslToAgent.AuthenticateAsServer(cert);
                    //sslToAgent.AuthenticateAsServer(cert, false, sslToServer.SslProtocol, false);

                    Logger.LogLine("Authenticated as server");

                    //Connection.CopyStream(sslToAgent, sslToServer);
                    //continue;

                    //while (streamToAgent.DataAvailable)
                    //{
                    //    sslToAgent.DataAvailable
                    //}

                    Logger.LogLine($"================================================");
                    HttpProxyMessage m = new HttpProxyMessage();
                    HttpProxyMessage.ReadMsgInto(sslToAgent, m.request);
                    Logger.LogLine($"Request recieved:{m.request.sb.ToString()}");
                    Logger.LogLine($"Request Body recieved:{Encoding.UTF8.GetString(m.request.body.datas.Span)}");
                    m.request.WriteTo(sslToServer);

                    Logger.LogLine($"================================================");
                    HttpProxyMessage.ReadMsgInto(sslToServer, m.response);
                    Logger.LogLine($"Response recieved:{m.response.sb.ToString()}");
                    Logger.LogLine($"Response Body recieved:{Encoding.UTF8.GetString(m.response.body.datas.Span)}");
                    m.response.WriteTo(sslToAgent);
                    origin.Dispose();
                }
                else
                {

                }
                agent.Dispose();
            }
        }
    }
}