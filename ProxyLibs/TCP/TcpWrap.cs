using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IziHardGames.Proxy.Tcp
{
    public class TcpWrap : IDisposable
    {
        public TcpClient Client { get; set; }

        public TcpWrap(TcpClient tcpClient)
        {
            Client = tcpClient;
            var stream = Client.GetStream();
            stream.WriteTimeout = -1;
            stream.ReadTimeout = -1;
        }

        public static TcpWrap Start(TcpClient tcpClient)
        {
            return new TcpWrap(tcpClient);
        }

        public bool CheckAlive()
        {
            return Client.Connected;
        }

        public void Dispose()
        {
            Client.Dispose();
        }

        public void Close()
        {
            Client.Dispose();
        }
    }
}