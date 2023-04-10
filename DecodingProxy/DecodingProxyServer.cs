// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using HttpDecodingProxy.http;
using IziHardGames.Proxy.Sniffing.Http;
using IziHardGames.Proxy.TcpDecoder;
using IziHardGames.Tls;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IziHardGames.Proxy
{
    public class DecodingProxyServer : IDisposable
    {
        private X509Certificate2 caRootCert;
        public X509Certificate2 CaRootCert => caRootCert;
        private readonly TcpServer tcpServer = new TcpServer();

        public GlobalProxySettings settings = new GlobalProxySettings();
        public readonly ManagerForHttpClientForIntercepting managerForHttpClientForIntercepting = new ManagerForHttpClientForIntercepting();
        public readonly ManagerForHttpMessages managerForHttpMessages = new ManagerForHttpMessages();
        public readonly ManagerForHttpAgentConnection managerForConnections = new ManagerForHttpAgentConnection();

        private object lockList = new object();
        public readonly CertManager certManager = new CertManager();
        private static List<string> sniffList = new List<string>();
        public ulong ticks;

        public void Start()
        {
            caRootCert = CertManager.LoadPemFromFile(@"C:\Users\ngoc\Documents\[Projects] C#\DecodingProxy\DecodingProxy\cert\IziHardGames_CA_CERT.pem", @"C:\Users\ngoc\Documents\[Projects] C#\DecodingProxy\DecodingProxy\cert\IziHardGames_CA_KEY.pem");
            tcpServer.OnClientConnectEvent += AcceptClient;
            tcpServer.OnClientConnectSslEvent += AcceptClientSsl;

            Logger.SetFilter("www.google.com");

            Settings.IsLogToFile = false;

            DecodingProxyServerConfig.EnsureConfigExist();

            foreach (var host in DecodingProxyServerConfig.Singleton.HostsForSniffing)
            {
                Logger.SetFilter(host);
            }

            //ManagerForTasks.Watch(Task.Run(() => tcpServer.Start()));
            ManagerForTasks.Watch(Task.Run(() => tcpServer.StartSSL()));

            while (true)
            {
                ManagerForTasks.CkeckErrors();
                Update();
            }
        }
        private void AcceptClient(TcpDecodingClient obj)
        {
            Logger.LogLine($"HTTP Client Accepted");
            var task = Task.Run(() =>
                   {
                       try
                       {
                           StartMsgMode(obj);
                       }
                       catch (Exception ex)
                       {
                           Logger.LogException(ex);
                       }
                   });
            ManagerForTasks.Watch(task);
        }
        private void AcceptClientSsl(TcpDecodingClient obj)
        {
            Logger.LogLine($"HTTPS Client Accepted");

            var task = Task.Run(() =>
                    {
                        StartMsgMode(obj);
                    });
            ManagerForTasks.Watch(task);
        }

        public void Update()
        {
            managerForHttpClientForIntercepting.Update();
            ticks++;
        }

        private void StartMsgMode(TcpDecodingClient tcpDecodingClient)
        {
            var client = tcpDecodingClient.Client;
            var stream = client.GetStream();
            var initMsg = managerForHttpMessages.Initiate(stream);
            string host = initMsg.request.fields.Host;
            Logger.LogLine($"Init Msg:{host}. Method:{initMsg.request.fields.Method}{Environment.NewLine}{initMsg.request.fields.ToStringInfo()}", ConsoleColor.Yellow);

            initMsg.IsHttps = initMsg.request.fields.IsConnectRequired;

            /// http or https decided by <see cref="Http11Fields.Host"/>. Connection is trusted to be appropriate
            if (managerForHttpClientForIntercepting.CreateV1(this, caRootCert, tcpDecodingClient, initMsg, out HttpClientForIntercepting httpClient))
            {
                Logger.LogLine($"Connection to {host} allocated");

                HttpAgentConnection agent = managerForConnections.Rent(host);
                agent.Initilize(client, httpClient, managerForHttpMessages, caRootCert);
                initMsg.agent = agent;
                httpClient.StartOrUpdateDequeueMsgMode(initMsg, managerForConnections, managerForHttpMessages);

                if (initMsg.request.fields.IsMethod(WebRequestMethods.Http.Connect))
                {
                    initMsg.Dispose();
                    agent.Run();
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            else
            {
                Logger.LogLine($"Duplicate connection disposed", ConsoleColor.Cyan);
                client.Dispose();
            }
        }

        private void StartTunnelMode(TcpDecodingClient tcpDecodingClient)
        {
            InterceptorHttp interceptor = new InterceptorHttp(tcpDecodingClient, managerForHttpClientForIntercepting, managerForHttpMessages, this);
            ManagerForTasks.Watch(Task.Run(interceptor.Start));
        }
        public void Dispose()
        {
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

        public static void Run()
        {
            using (DecodingProxyServer decodingProxy = new DecodingProxyServer())
            {
                DecodingProxyServer.Sniff("www.google.com");
                decodingProxy.Start();
            }
        }

        public static void Test()
        {
            TcpListener tcpListener = new TcpListener(TcpServer.DEFAULT_PORT_SSL);
            tcpListener.Start();

            while (true)
            {
                var agent = tcpListener.AcceptTcpClient();
                TcpDecodingClient tdc = new TcpDecodingClient(agent);

                HttpProxyMessage msg = new HttpProxyMessage();
                var streamToAgent = agent.GetStream();
                msg.ReadMsgInto(streamToAgent, agent.GetStream(), msg.request);

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

                    Stream streamToOrigin = origin.GetStream();
                    SslStream sslToServer = new SslStream(streamToOrigin);
                    sslToServer.AuthenticateAsClient(msg.request.fields.HostAddress);
                    Logger.LogLine("Authenticated as client");

                    var serverCert = sslToServer.RemoteCertificate;
                    var caCert = CertManager.LoadPemFromFile(@"C:\Users\ngoc\Documents\[Projects] C#\DecodingProxy\DecodingProxy\cert\IziHardGames_CA_CERT.pem", @"C:\Users\ngoc\Documents\[Projects] C#\DecodingProxy\DecodingProxy\cert\IziHardGames_CA_KEY.pem");
                    var cert = CertManager.GenerateCertEndpoint((X509Certificate2)serverCert, caCert, DateTime.Now.AddYears(5));
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
                    m.ReadMsgInto(sslToAgent, agent.GetStream(), m.request);
                    Logger.LogLine($"Request recieved:{m.request.sb.ToString()}");
                    Logger.LogLine($"Request Body recieved:{Encoding.UTF8.GetString(m.request.body.datas.Span)}");
                    m.request.Write(sslToServer);

                    Logger.LogLine($"================================================");
                    m.ReadMsgInto(sslToServer, origin.GetStream(), m.response);
                    Logger.LogLine($"Response recieved:{m.response.sb.ToString()}");
                    Logger.LogLine($"Response Body recieved:{Encoding.UTF8.GetString(m.response.body.datas.Span)}");
                    m.response.Write(sslToAgent);
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