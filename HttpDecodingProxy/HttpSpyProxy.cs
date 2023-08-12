using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Core;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Sniffing.ForHttp;
using IziHardGames.Proxy.Tcp.Tls;
using IziHardGames.Proxy.TcpDecoder;
using IziHardGames.Tls;
using Microsoft.Extensions.Logging;
//using ConnectionsToDomain = IziHardGames.Proxy.Tcp.ConnectionsToDomain<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>;
//using CtdTls = IziHardGames.Proxy.Tcp.Tls.ConnectionsToDomainTls<IziHardGames.Proxy.Tcp.SocketWrap, IziHardGames.Libs.Networking.Pipelines.SocketWrapUpgradeTls>;
//using ManagerConnectionsToDomain = IziHardGames.Libs.ObjectsManagment.ManagerBase<string, IziHardGames.Proxy.Tcp.ConnectionsToDomain<IziHardGames.Libs.Networking.Pipelines.TcpClientPiped>, (string, int)>;
//using ManagerConnectionsToDomainSsl = IziHardGames.Libs.ObjectsManagment.ManagerBase<string, IziHardGames.Proxy.Tcp.Tls.ConnectionsToDomainTls<IziHardGames.Proxy.Tcp.SocketWrap, IziHardGames.Libs.Networking.Pipelines.SocketWrapUpgradeTls>, (string, int)>;

namespace IziHardGames.Proxy.Http
{
    public class HttpSpyProxy : IProxy, IDisposable
    {
        private X509Certificate2 caRootCert;
        public X509Certificate2 CaRootCert => caRootCert;

        public GlobalProxySettings settings = new GlobalProxySettings();

        //public readonly ManagerConnectionsToDomain manager;
        //public readonly ManagerConnectionsToDomainSsl managerSsl;

        private object lockList = new object();
        public CertManager certManager;
        private static List<string> sniffList = new List<string>();
        public ulong ticks;
        private ConsumingProvider consumingProvider;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ILogger logger;

        private ClientHandlerPipedHttp clientHandlerPipedHttp;
        private ClientHandlerPipedHttpsV2 handlerPipedHttpsV2;
        private ClientHandlerPipedHttpsForDirrectConnect handlerDirect = new ClientHandlerPipedHttpsForDirrectConnect();

        private IChangeNotifier<IConnectionData>? monitorForConnections;

        public HttpSpyProxy(ILogger<HttpSpyProxy> logger, IChangeNotifier<IConnectionData> monitorForConnections)
        {
            this.logger = logger;
            this.monitorForConnections = monitorForConnections!;

            //manager = new ManagerConnectionsToDomain((key, address) =>
            //{
            //    var pool = PoolObjectsConcurent<ConnectionsToDomain>.Shared;
            //    var rent = pool.Rent();
            //    rent.BindToPool(pool);
            //    rent.Key = key;
            //    rent.UpdateAddress(address.Item1, address.Item2);
            //    rent.Start();
            //    rent.RegistRuturnToManager(manager!.Return);
            //    rent.monitor = monitorForConnections;
            //    return rent;
            //});

            //managerSsl = new ManagerConnectionsToDomainSsl((key, options) =>
            //{
            //    var pool = PoolObjectsConcurent<CtdTls>.Shared;
            //    var rent = pool.Rent();
            //    rent.Key = key;
            //    rent.UpdateAddress(options.Item1, options.Item2);
            //    rent.BindToPool(pool);
            //    rent.RegistRuturnToManager(managerSsl!.Return);
            //    rent.Start();
            //    rent.monitor = monitorForConnections;
            //    return rent;
            //});
        }

        private void Init(CertManager certManager, ConsumingProvider consumingProvider)
        {
            Console.WriteLine($"это на русском языке");
            this.certManager = certManager;

            Settings.IsLogToFile = false;

            DecodingProxyServerConfig.Singleton = new DecodingProxyServerConfig();

            foreach (var host in DecodingProxyServerConfig.Singleton.HostsForSniffing)
            {
                Logger.SetFilter(host);
            }

            try
            {
                caRootCert = CertManager.LoadPemFromFile(ConfigJson.path_ca_cert, ConfigJson.path_ca_key);
            }
            catch (Exception ex)
            {
                Break();
                Logger.LogException(ex);
                throw ex;
            }
            //clientHandlerPipedHttp = new ClientHandlerPipedHttp(consumingProvider, monitorForConnections!, manager);
            //handlerPipedHttpsV2 = new ClientHandlerPipedHttpsV2(consumingProvider, monitorForConnections!, managerSsl, caRootCert, certManager);
        }

        private async Task RunWithPipedTcp(CancellationToken token, int port)
        {
            throw new System.NotImplementedException();
            //using (TcpServerSocketBased<TcpClientPiped> server = new())
            //{
            //    logger.LogInformation($"Begin http spy proxy port:{port}");
            //    await server.Run("localhost", port, logger, clientHandlerPipedHttp, PoolObjectsConcurent<TcpClientPiped>.Shared, token).ConfigureAwait(false);
            //}
        }
        private async Task RunWithPipedTcpAndDetectionSsl(CancellationToken token, int port)
        {
            // if Method CONNECT is called than upgrade connection.
            TcpServer<SocketWrap> server = new(port, PoolObjectsConcurent<SocketWrap>.Shared, handlerDirect);
            await server.Run(logger, token);
        }
        private async Task RunWithPipedTcpSslV2(CancellationToken token, int port)
        {
            logger.LogInformation($"Begin https spy proxy port:{port}");
            TcpServer<SocketWrap> server = new(port, PoolObjectsConcurent<SocketWrap>.Shared, handlerPipedHttpsV2);
            await server.Run(logger, token);
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

        public async Task Run(ConsumingProvider consumingProvider)
        {
            Logger.LogLine($"STart Proxy decoding");
            this.consumingProvider = consumingProvider;
            ConfigJson.EnsureConfigExist();
            UpdateSettings();
            CertManager.CreateDefault(ConfigJson.PathCertForged, ConfigJson.PathCertOriginal);
            Init(CertManager.Shared, consumingProvider);
            var token = cts.Token;

            //var task1 = Task.Run(async () => await RunWithPipedTcp(token, 49702), token);
            //var task2 = Task.Run(async () => await RunWithPipedTcpSslV2(token, 60121), token);
            var task3 = Task.Run(async () => await RunWithPipedTcpAndDetectionSsl(token, 60121), token);
            await task3.ConfigureAwait(false);
            //var task3 = Task.Run(async () => await RunWithPipedTcpSslV2(token, 60122), token);
            //var task4 = Task.Run(async () => await RunWithPipedTcpSslV2(token, 443), token);
            //var task5 = Task.Run(async () => await RunWithPipedTcp(token, 80), token);

            //await Task.WhenAll(task1, task2).ConfigureAwait(false);
            //await Task.WhenAll(task3, task4, task5).ConfigureAwait(false);
        }

        public async Task Break()
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

        //#if DEBUG
        //        public static async Task Test()
        //        {
        //            TcpListener tcpListener = new TcpListener(60121);
        //            tcpListener.Start();

        //            while (true)
        //            {
        //                var agent = tcpListener.AcceptSocket();
        //                SocketWrap tdc = new SocketWrap();
        //                tdc.Wrap(agent);

        //                HttpProxyMessage msg = new HttpProxyMessage();
        //                var streamToAgent = agent.GetStream();
        //                HttpProxyMessage.ReadMsgInto(streamToAgent, msg.request);
        //                var options = msg.ToStartOptions();

        //                Logger.LogLine(msg.request.sb.ToString());
        //                Logger.LogLine("------------------------------------------------");

        //                if (!msg.request.fields.InitLine.Contains("google") && !msg.request.fields.InitLine.Contains("CONNECT:"))
        //                {
        //                    agent.Close();
        //                    continue;
        //                }

        //                if (msg.request.fields.IsConnectRequired)
        //                {
        //                    Logger.LogLine("Connection required pass");

        //                    /// https://httpwg.org/specs/rfc9112.html#message.body.length<br/>
        //                    /// Any 2xx (Successful) response to a CONNECT request implies that the connection will become 
        //                    /// a tunnel immediately after the empty line that concludes the header fields. 
        //                    /// A client MUST ignore any Content-Length or Transfer-Encoding header fields received in such a message.
        //                    streamToAgent.Write(HttpProxyMessage.response200);
        //                    Logger.LogLine("Sended 200 Ok to client");

        //                    TcpClient origin = new TcpClient();
        //                    origin.Connect(msg.request.fields.HostAddress, msg.request.fields.HostPort);
        //                    Logger.LogLine("Connected to Origin");

        //                    /// do not access to <see cref="Socket.Available"/> after creating <see cref="SslStream"/>. Somehow it closing connection
        //                    Stream streamToOrigin = origin.GetStream();
        //                    SslStream sslToServer = new SslStream(streamToOrigin);
        //                    sslToServer.AuthenticateAsClient(msg.request.fields.HostAddress);
        //                    Logger.LogLine("Authenticated as client");

        //                    var serverCert = sslToServer.RemoteCertificate;
        //                    var caCert = CertManager.LoadPemFromFile(ConfigJson.path_ca_cert, ConfigJson.path_ca_key);
        //                    var cert = await CertManager.Shared.ForgedGetOrCreateCertFromCacheAsync((X509Certificate2)serverCert, caCert).ConfigureAwait(false);
        //                    Logger.LogLine("Forged SSL");

        //                    SslStream sslToAgent = new SslStream(streamToAgent);
        //                    sslToAgent.AuthenticateAsServer(cert);
        //                    //sslToAgent.AuthenticateAsServer(cert, false, sslToServer.SslProtocol, false);

        //                    Logger.LogLine("Authenticated as server");

        //                    //Connection.CopyStream(sslToAgent, sslToServer);
        //                    //continue;

        //                    //while (streamToAgent.DataAvailable)
        //                    //{
        //                    //    sslToAgent.DataAvailable
        //                    //}

        //                    Logger.LogLine($"================================================");
        //                    HttpProxyMessage m = new HttpProxyMessage();
        //                    HttpProxyMessage.ReadMsgInto(sslToAgent, m.request);
        //                    Logger.LogLine($"Request recieved:{m.request.sb.ToString()}");
        //                    Logger.LogLine($"Request Body recieved:{Encoding.UTF8.GetString(m.request.body.datas.Span)}");
        //                    m.request.WriteTo(sslToServer);

        //                    Logger.LogLine($"================================================");
        //                    HttpProxyMessage.ReadMsgInto(sslToServer, m.response);
        //                    Logger.LogLine($"Response recieved:{m.response.sb.ToString()}");
        //                    Logger.LogLine($"Response Body recieved:{Encoding.UTF8.GetString(m.response.body.datas.Span)}");
        //                    m.response.WriteTo(sslToAgent);
        //                    origin.Dispose();
        //                }
        //                else
        //                {

        //                }
        //                agent.Dispose();
        //            }
        //        }
        //#endif
    }
}