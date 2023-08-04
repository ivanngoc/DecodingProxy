using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Test
{

    internal class TcpInfo
    {
        internal static async Task RunUdp(int port)
        {
            var address = IPAddress.Loopback;
            var endpoint = new IPEndPoint(address, port);
            var udpClient = new UdpClient(port);
            udpClient.Connect(endpoint);

            while (true)
            {
                var res = await udpClient.ReceiveAsync();
                Console.WriteLine($"Recived");
                byte[] data = res.Buffer;
                Console.WriteLine(Encoding.UTF8.GetString(data, 0, data.Length));
            }
        }
        internal static async Task RunHex(int port)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();

            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine($"Client accepted");
                await Task.Run(async () =>
                   {
                       var stream = client.GetStream();
                       var buffer = new byte[1024];
                       while (true)
                       {
                           var b = stream.ReadByte();
                           if (b > 0)
                           {
                               var pair = ParseByte.ByteToHex((byte)b);
                               Console.Write($"0x{pair.Item1}{pair.Item2} ");
                           }
                       }
                   });
            }
        }
        internal static async Task Run(int port)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();

            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine($"Client accepted");
                Task.Run(async () =>
                {
                    var stream = client.GetStream();
                    var buffer = new byte[1024];
                    while (true)
                    {
                        var count = await stream.ReadAsync(buffer);
                        if (count > 0)
                            Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, count));
                    }
                });
            }
        }
    }
}
