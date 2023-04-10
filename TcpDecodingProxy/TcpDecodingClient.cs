using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TcpDecodingClient : IDisposable
    {        
        public TcpClient Client { get; set; }

        public TcpDecodingClient(TcpClient tcpClient)
        {
            this.Client = tcpClient;
        }

        internal static TcpDecodingClient Start(TcpClient tcpClient)
        {
            return new TcpDecodingClient(tcpClient);
        }

        public bool CheckAlive()
        {
            return Client.Connected;
        }

        public void Dispose()
        {
            Client.Dispose();
        }

        public void ListenDebugV3()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Begin Debug Listening With Decomplress");
            var stream = Client.GetStream();
            var decompressor = new GZipStream(stream, CompressionMode.Decompress);
            TextReader textReader = new StreamReader(stream, Encoding.UTF8);

            while (Client.Connected)
            {
                var line = textReader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    Console.WriteLine(line);
                }
            }
        }
        private void ListenDebugV2()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Begin Debug Listening");
            var stream = Client.GetStream();
            TextReader textReader = new StreamReader(stream);

            while (true)
            {
                string line = textReader.ReadLine();

                if (string.IsNullOrEmpty(line))
                {
                    return;
                }
                Console.WriteLine(line);
            }
        }
        private void ListenDebug()
        {
            var stream = Client.GetStream();
            TextReader textReader = new StreamReader(stream, Encoding.ASCII);

            while (Client.Connected)
            {
                int val = textReader.Read();

                if (val < 0)
                {
                    Console.WriteLine($"CONNECTION LOST");
                    break;
                }
                Console.Write((char)val);
            }
        }
    }
}