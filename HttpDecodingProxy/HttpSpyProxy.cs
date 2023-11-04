using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Core;
using IziHardGames.Libs.Concurrency;
using IziHardGames.Libs.HttpCommon;
using IziHardGames.Libs.HttpCommon.Common;
using IziHardGames.Libs.HttpCommon.Monitoring;
using IziHardGames.Libs.gRPC.Services;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Tls;
using Microsoft.Extensions.Logging;

namespace IziHardGames.Proxy.Http
{
    public class HttpSpyProxy : IProxy, IDisposable
    {
        private X509Certificate2 caRootCert;
        public X509Certificate2 CaRootCert => caRootCert;

        public GlobalProxySettings settings = new GlobalProxySettings();
        public CertManager certManager;

        private readonly ILogger<HttpSpyProxy> logger;
        private readonly IChangeNotifier<IConnectionData>? monitorForConnections;
        private readonly HttpConsumer consumer;


        public ulong ticks;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private static List<string> sniffList = new List<string>();
        private ConsumingProvider consumingProvider;

        public HttpSpyProxy(ILogger<HttpSpyProxy> logger, IChangeNotifier<IConnectionData> monitorForConnections, HttpConsumer consumer, GrpcHubService grpcHubService, HttpEventPublisherGrpc publisher)
        {
            this.logger = logger;
            this.monitorForConnections = monitorForConnections!;
            this.consumer = consumer;
            HttpEventCenter.SetEventConsumer(publisher);
        }
        private void Init(CertManager certManager, ConsumingProvider consumingProvider)
        {
            Console.WriteLine($"это на русском языке");
            this.certManager = certManager;

            Settings.IsLogToFile = false;

            DecodingProxyServerConfig.Singleton = new DecodingProxyServerConfig();

            foreach (var host in DecodingProxyServerConfig.Singleton.HostsForSniffing)
            {
                MyLogger.SetFilter(host);
            }

            try
            {
                caRootCert = CertManager.LoadPemFromFile(ConfigJson.path_ca_cert, ConfigJson.path_ca_key);
            }
            catch (Exception ex)
            {
                Break();
                MyLogger.LogException(ex);
                throw ex;
            }
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
            MyLogger.LogLine($"Start Proxy decoding");
            this.consumingProvider = consumingProvider;
            ConfigJson.EnsureConfigExist();
            UpdateSettings();
            CertManager.CreateSharedCa();
            Init(CertManager.Shared, consumingProvider);
            var token = cts.Token;

            var task1 = Task.Run(async () => await RunHttp(49702, token), token);
            var task2 = Task.Run(async () => await RunHttps(60121, token), token);

            await task1.ConfigureAwait(false);
            await task2.ConfigureAwait(false);
        }
        private async Task RunHttp(int port, CancellationToken ct = default)
        {
            MonitorForTasks monitor = new MonitorForTasks(logger);
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (!ct.IsCancellationRequested)
            {
                var socket = await listener.AcceptSocketAsync(ct).ConfigureAwait(false);
                Task task = HttpProxyProcessor.HandleSocket(consumer, socket);
                monitor.Watch(task);
            }
        }
        private async Task RunHttps(int port, CancellationToken ct = default)
        {
            MonitorForTasks monitor = new MonitorForTasks(logger);
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            while (!ct.IsCancellationRequested)
            {
                var socket = await listener.AcceptSocketAsync(ct).ConfigureAwait(false);
                Task task = HttpProxyProcessor.HandleSocket(consumer, socket);
                monitor.Watch(task);
            }
        }
        public async Task Break()
        {
            cts.Cancel();
            MyLogger.LogException(new ApplicationException($"{nameof(HttpSpyProxy)} Has Stopped"));
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