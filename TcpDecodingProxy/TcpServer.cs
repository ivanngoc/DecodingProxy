using System.Net;
using System.Net.Sockets;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TcpServer : IDisposable
    {
        public const int port = 49702;
        public const int DEFAULT_PORT_SSL = 60121;

        private TcpListener tcpListener;
        private List<TcpDecodingClient> clients = new List<TcpDecodingClient>();
        public event Action<TcpDecodingClient> OnClientConnectEvent;
        public event Action<TcpDecodingClient> OnClientConnectSslEvent;

        public List<TcpDecodingClient> GetClients()
        {
            return clients;
        }

        public void Start()
        {
            Console.WriteLine($"HTTP Server Initilizing...");

            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(iPAddress, port);
            tcpListener.Start();

            var task = Task.Run(CheckAlive);

            Console.WriteLine($"HTTP Server Started");
            while (true)
            {
                var client = TcpDecodingClient.Start(tcpListener.AcceptTcpClient());

                lock (clients)
                {
                    clients.Add(client);
                }
                OnClientConnectEvent?.Invoke(client);
            }
        }
        public void StartSSL()
        {
            Console.WriteLine($"HTTPS Server Initilizing...");

            IPAddress iPAddress = IPAddress.Parse("127.0.0.1");
            tcpListener = new TcpListener(iPAddress, DEFAULT_PORT_SSL);
            tcpListener.Start();
            Console.WriteLine($"HTTPS Server Started");

            var task = Task.Run(CheckAlive);

            while (true)
            {
                var client = TcpDecodingClient.Start(tcpListener.AcceptTcpClient());

                lock (clients)
                {
                    clients.Add(client);
                }
                OnClientConnectSslEvent?.Invoke(client);
            }
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

        public static void Run()
        {
            TcpServer tcpServer = new TcpServer();
            tcpServer.StartSSL();
        }
    }
}