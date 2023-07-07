using IziHardGames.Proxy.Tcp;
using System.Net;
using System.Net.Sockets;
using AcceptClient = System.Func<IziHardGames.Proxy.Tcp.TcpWrap, System.Threading.Tasks.Task<IziHardGames.Proxy.Tcp.TcpWrap>>;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TcpServer : IDisposable
    {
        public const int port = 49702;
        public const int DEFAULT_PORT_SSL = 60121;

        private List<TcpWrap> clients = new List<TcpWrap>();
        public event Action<TcpWrap> OnClientConnectEvent;
        public event Action<TcpWrap> OnClientConnectSslEvent;

        public List<TcpWrap> GetClients()
        {
            return clients;
        }

        public void Start(CancellationToken token)
        {
            Console.WriteLine($"HTTP Server Initilizing...");

            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            var tcpListener = new TcpListener(iPAddress, port);
            tcpListener.Start();

            var task = Task.Run(CheckAlive);

            Console.WriteLine($"HTTP Server Started");

            while (!token.IsCancellationRequested)
            {
                var client = TcpWrap.Start(tcpListener.AcceptTcpClient());

                lock (clients)
                {
                    clients.Add(client);
                }
                OnClientConnectEvent?.Invoke(client);
            }

            tcpListener.Stop();
        }
        public async Task StartSSL(AcceptClient handler, CancellationToken token)
        {
            Console.WriteLine($"HTTPS Server Initilizing...");

            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            var tcpListener = new TcpListener(iPAddress, DEFAULT_PORT_SSL);
            tcpListener.Start();
            Console.WriteLine($"HTTPS Server Started");

            List<Task<TcpWrap>> tasks = new List<Task<TcpWrap>>(128);

            var t1 = Task.Run(async () =>
                {
                    while (true)
                    {
                        if (tasks.Count > 0)
                        {
                            var task = await Task.WhenAny(tasks).ConfigureAwait(false);

                            lock (clients)
                            {
                                clients.Remove(task.Result);
                                tasks.Remove(task);
                            }
                        }
                        await Task.Delay(5000);
                    }
                });

            while (!token.IsCancellationRequested)
            {
                var client = TcpWrap.Start(await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false));
                var task = handler(client);

                lock (clients)
                {
                    tasks.Add(task);
                    clients.Add(client);
                }
            }
            await Task.WhenAll(t1).ConfigureAwait(false);
            tcpListener.Stop();
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void CheckAlive()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (!clients[i].CheckAlive())
                {
                    clients.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}