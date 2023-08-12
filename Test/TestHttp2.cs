using HttpDecodingProxy.ForHttp;
using IziHardGames.Lib.Networking.Exceptions;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.ForHttp20;
using IziHardGames.Libs.ForHttp20.Https.Extensions;
using IziHardGames.Libs.Networking.DevTools;
using IziHardGames.Proxy.TcpDecoder;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    internal class TestHttp2
    {
        public static string hostIpCheck = $"www.http2demo.io";
        public static string host = $"www.google.com";
        public static string ip = $"216.58.207.196";
        public static IPAddress[] adresses;

        static TestHttp2()
        {
            var v1 = IPAddress.Parse("195.181.170.19");
            var v2 = IPAddress.Parse("156.146.33.140");
            var v3 = IPAddress.Parse("156.146.33.138");
            var v4 = IPAddress.Parse("195.181.175.40");
            var v5 = IPAddress.Parse("195.181.175.15");

            adresses = new IPAddress[] { v1, v2, v3, v4, v5 };
        }
        public static async Task ShowIp()
        {
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(hostIpCheck);

            foreach (var address in addresses)
            {
                Console.WriteLine(address);
            }
            /*
                195.181.170.19
                156.146.33.140
                156.146.33.138
                195.181.175.40
                195.181.175.15
                2a02:6ea0:c700::10
                2a02:6ea0:c700::19
                2a02:6ea0:c700::18
                2a02:6ea0:c700::11
                2a02:6ea0:c700::17
             */
        }
        public static async Task RunHttp2Ssl()
        {
            TcpClient client = new TcpClient();

            await client.ConnectAsync(host, 443);

            var stream = new DebugStream(client.GetStream(), ELogType.None);
            SslStream sslStream = new SslStream(stream);

            ///<see cref="SecurityProtocolType.SystemDefault"/>

            SslClientAuthenticationOptions options = new SslClientAuthenticationOptions()
            {
                TargetHost = host,
                ApplicationProtocols = new List<SslApplicationProtocol>() { SslApplicationProtocol.Http2, },
                EnabledSslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13,
                //EnabledSslProtocols = SslProtocols.Tls12,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck,
                //EncryptionPolicy = EncryptionPolicy.RequireEncryption,
            };
            await sslStream.AuthenticateAsClientAsync(options);

            Console.WriteLine($"Authneticated");

            Task t1 = Task.Run(async () =>
            {
                byte[] buffer = new byte[(1 << 20) * 8];
                while (true)
                {
                    int readed = await sslStream.ReadAsync(buffer);
                    if (readed > 0)
                    {
                        Console.WriteLine($"Task Read Ssl Stream:");
                        for (int i = 0; i < readed; i++)
                        {
                            Console.Write(ParseByte.ByteToHexFormated(buffer[i]));
                        }
                        Console.WriteLine($"Task Read Ssl Stream As String: {new ReadOnlySpan<byte>(buffer, 0, readed).ToStringUtf8()}");
                    }
                }
            });

            var t3 = sslStream.WriteAsync(ConstantsHttp20.clientPrefaceBytes);
            await t3.ConfigureAwait(false);
            Console.WriteLine("Preface sended for Http2 SSL");
            HttpFrame frame = HttpFrame.Settings;
            frame.streamIdentifier = 1;
            frame.WriteThisTo(sslStream);
            Console.WriteLine("Settings after preface sended for Http2 SSL");
            //sslStream.SendHttp2RequestAsync();


            string req = "GET /search?hl=ru&q=jj&tbm=isch&source=lnms&sa=X&ved=2ahUKEwj4hOrewr-AAxVlHxAIHegABTYQ0pQJegQIDxAB&biw=1920&bih=927&dpr=1 HTTP/2\r\nHost: www.google.com\r\nUser-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\nAccept-Language: ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3\r\nAccept-Encoding: gzip, deflate, br\r\nReferer: https://www.google.com/\r\nDNT: 1\r\nConnection: keep-alive\r\nCookie: 1P_JAR=2023-8-4-8; AEC=Ad49MVGnY4ec-pKxaySAlfvHzXzXZKj_aTzO5kODlYiRWQxmmVL3JcSUBA; NID=511=K5UZnNgc1hm5EnVOC4xFTqYCKpCDEJNtSinrBFJXhm7vjFCIn40wp-iYshijq9GK6ChTGZ7dcbIJoOQSXWwncePvhii5_OaVHaoFCKleMPAyoJUcVpz66jAQv_1OI8l8Yq8S0EzT9YKV6XNWH7o3zSKtc4htclOBF24zvQmUpB7RcZbc1w-Cdw; OTZ=7144666_28_28_123540_24_436020\r\nUpgrade-Insecure-Requests: 1\r\nSec-Fetch-Dest: document\r\nSec-Fetch-Mode: navigate\r\nSec-Fetch-Site: same-origin\r\nSec-Fetch-User: ?1\r\n\r\n";
            var t2 = sslStream.WriteAsync(Encoding.UTF8.GetBytes(req));

            await t1.ConfigureAwait(false);
            await t2.ConfigureAwait(false);
        }
        public static async Task RunTlsParse()
        {
            var clientToServer = new byte[] { 0x16, 0x03, 0x03, 0x00, 0xBE, 0x01, 0x00, 0x00, 0xBA, 0x03, 0x03, 0x64, 0xD0, 0x6D, 0xE2, 0xE1, 0xA4, 0x37, 0x8A, 0xD8, 0x78, 0xA8, 0x17, 0x57, 0xA5, 0x89, 0x3D, 0x52, 0x4D, 0x7D, 0x34, 0x66, 0xA6, 0xE2, 0x4D, 0x8F, 0x3E, 0xBC, 0x6D, 0x08, 0x7B, 0x77, 0xE0, 0x00, 0x00, 0x34, 0xC0, 0x2C, 0xC0, 0x2B, 0xC0, 0x30, 0xC0, 0x2F, 0x00, 0x9F, 0x00, 0x9E, 0xC0, 0x24, 0xC0, 0x23, 0xC0, 0x28, 0xC0, 0x27, 0xC0, 0x0A, 0xC0, 0x09, 0xC0, 0x14, 0xC0, 0x13, 0x00, 0x9D, 0x00, 0x9C, 0x00, 0x3D, 0x00, 0x3C, 0x00, 0x35, 0x00, 0x2F, 0x00, 0x0A, 0xC1, 0x00, 0xC1, 0x01, 0xC1, 0x02, 0xFF, 0x85, 0x00, 0x81, 0x01, 0x00, 0x00, 0x5D, 0x00, 0x00, 0x00, 0x13, 0x00, 0x11, 0x00, 0x00, 0x0E, 0x77, 0x77, 0x77, 0x2E, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x00, 0x0A, 0x00, 0x08, 0x00, 0x06, 0x00, 0x1D, 0x00, 0x17, 0x00, 0x18, 0x00, 0x0B, 0x00, 0x02, 0x01, 0x00, 0x00, 0x0D, 0x00, 0x1A, 0x00, 0x18, 0x08, 0x04, 0x08, 0x05, 0x08, 0x06, 0x04, 0x01, 0x05, 0x01, 0x02, 0x01, 0x04, 0x03, 0x05, 0x03, 0x02, 0x03, 0x02, 0x02, 0x06, 0x01, 0x06, 0x03, 0x00, 0x23, 0x00, 0x00, 0x00, 0x10, 0x00, 0x05, 0x00, 0x03, 0x02, 0x68, 0x32, 0x00, 0x17, 0x00, 0x00, 0xFF, 0x01, 0x00, 0x01, 0x00 };
            var serverToClient = new byte[] { 0x48, 0x54, 0x54, 0x50, 0x2F, 0x31, 0x2E, 0x30, 0x20, 0x34, 0x30, 0x30, 0x20, 0x42, 0x61, 0x64, 0x20, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x0D, 0x0A, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x2D, 0x4C, 0x65, 0x6E, 0x67, 0x74, 0x68, 0x3A, 0x20, 0x35, 0x34, 0x0D, 0x0A, 0x43, 0x6F, 0x6E, 0x74, 0x65, 0x6E, 0x74, 0x2D, 0x54, 0x79, 0x70, 0x65, 0x3A, 0x20, 0x74, 0x65, 0x78, 0x74, 0x2F, 0x68, 0x74, 0x6D, 0x6C, 0x3B, 0x20, 0x63, 0x68, 0x61, 0x72, 0x73, 0x65, 0x74, 0x3D, 0x55, 0x54, 0x46, 0x2D, 0x38, 0x0D, 0x0A, 0x44, 0x61, 0x74, 0x65, 0x3A, 0x20, 0x4D, 0x6F, 0x6E, 0x2C, 0x20, 0x30, 0x37, 0x20, 0x41, 0x75, 0x67, 0x20, 0x32, 0x30, 0x32, 0x33, 0x20, 0x30, 0x34, 0x3A, 0x30, 0x36, 0x3A, 0x35, 0x38, 0x20, 0x47, 0x4D, 0x54, 0x0D, 0x0A, 0x0D, 0x0A, 0x3C, 0x68, 0x74, 0x6D, 0x6C, 0x3E, 0x3C, 0x74, 0x69, 0x74, 0x6C, 0x65, 0x3E, 0x45, 0x72, 0x72, 0x6F, 0x72, 0x20, 0x34, 0x30, 0x30, 0x20, 0x28, 0x42, 0x61, 0x64, 0x20, 0x52, 0x65, 0x71, 0x75, 0x65, 0x73, 0x74, 0x29, 0x21, 0x21, 0x31, 0x3C, 0x2F, 0x74, 0x69, 0x74, 0x6C, 0x65, 0x3E, 0x3C, 0x2F, 0x68, 0x74, 0x6D, 0x6C, 0x3E };

            Console.WriteLine($"Client as string: {clientToServer.ToStringUtf8()}");
            Console.WriteLine($"Server As String {serverToClient.ToStringUtf8()}");
            Console.WriteLine();
            Console.WriteLine("Client");
            TlsHelloFromClient.Read<IndexReaderForArray<byte>>(clientToServer);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Server");
            TlsHelloFromServer.Read<IndexReaderForArray<byte>>(serverToClient);
            Console.ReadLine();
        }
        public static async Task RunHttp2()
        {
            TcpClient client = new TcpClient();
            var host = $"google.com";
            await client.ConnectAsync(host, 80);
            var stream = client.GetStream();
            //SslStream sslStream = new SslStream(client.GetStream());
            //await sslStream.AuthenticateAsClientAsync(host);
            Console.WriteLine($"Authneticated");

            Task t1 = Task.Run(async () =>
            {
                byte[] buffer = new byte[(1 << 20) * 8];
                while (true)
                {
                    int readed = await stream.ReadAsync(buffer);
                    if (readed > 0)
                    {
                        for (int i = 0; i < readed; i++)
                        {
                            Console.Write(ParseByte.ByteToHexFormated(buffer[i]));
                        }
                        //Console.Write(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, readed)));
                    }
                }
            });

            var t3 = stream.WriteAsync(ConstantsHttp20.clientPrefaceBytes);
            await t3.ConfigureAwait(false);
            HttpFrame frame = HttpFrame.Settings;
            frame.streamIdentifier = 1;
            frame.WriteThisTo(stream);

            Console.ReadLine();


            string req = "GET /search?hl=ru&q=jj&tbm=isch&source=lnms&sa=X&ved=2ahUKEwj4hOrewr-AAxVlHxAIHegABTYQ0pQJegQIDxAB&biw=1920&bih=927&dpr=1 HTTP/1.1\r\nHost: www.google.com\r\nUser-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/115.0\r\nAccept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\nAccept-Language: ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3\r\nAccept-Encoding: gzip, deflate, br\r\nReferer: https://www.google.com/\r\nDNT: 1\r\nConnection: keep-alive\r\nCookie: 1P_JAR=2023-8-4-8; AEC=Ad49MVGnY4ec-pKxaySAlfvHzXzXZKj_aTzO5kODlYiRWQxmmVL3JcSUBA; NID=511=K5UZnNgc1hm5EnVOC4xFTqYCKpCDEJNtSinrBFJXhm7vjFCIn40wp-iYshijq9GK6ChTGZ7dcbIJoOQSXWwncePvhii5_OaVHaoFCKleMPAyoJUcVpz66jAQv_1OI8l8Yq8S0EzT9YKV6XNWH7o3zSKtc4htclOBF24zvQmUpB7RcZbc1w-Cdw; OTZ=7144666_28_28_123540_24_436020\r\nUpgrade-Insecure-Requests: 1\r\nSec-Fetch-Dest: document\r\nSec-Fetch-Mode: navigate\r\nSec-Fetch-Site: same-origin\r\nSec-Fetch-User: ?1\r\n\r\n";
            var t2 = stream.WriteAsync(Encoding.UTF8.GetBytes(req));


            await t1.ConfigureAwait(false);
            await t2.ConfigureAwait(false);
        }

        public static async Task RunProxy()
        {
            Console.WriteLine(Environment.StackTrace);
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 80);
            tcpListener.Start();

            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);
                var t1 = Task.Run(async () =>
                {
                    client.ReceiveTimeout = 15000;
                    client.ReceiveTimeout = 15000;

                    Console.WriteLine($"New Client");
                    var s = client.GetStream();
                    DebugStream stream = new DebugStream(s, ELogType.HexFormat);

                    TcpClient origin = new TcpClient();
                    await origin.ConnectAsync(adresses, 80);

                    var t2 = Task.Run(async () =>
                    {
                        byte[] buffer = new byte[(1 << 20) * 8];
                        var streamSend = origin.GetStream();
                        bool isZero = default;
                        while (true)
                        {
                            int readed = await stream.ReadAsync(buffer);
                            if (readed > 0)
                            {
                                isZero = false;
                                await streamSend.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, readed));
                            }
                            else
                            {
                                if (isZero) return;
                                isZero = true;
                                await Task.Delay(1000);
                            }
                        }
                    });

                    var t3 = Task.Run(async () =>
                    {
                        byte[] buffer = new byte[(1 << 20) * 8];
                        var streamSend = stream;
                        var streamRead = origin.GetStream();
                        bool isZero = default;

                        while (true)
                        {
                            //CancellationToken cancellationToken = CancellationToken.
                            int readed = await streamRead.ReadAsync(buffer);
                            if (readed > 0)
                            {
                                isZero = false;
                                await streamSend.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, readed));
                            }
                            else
                            {
                                if (isZero) return;
                                isZero = true;
                                await Task.Delay(1000);
                            }
                        }
                    });
                    try
                    {
                        await Task.WhenAll(t2, t3).ConfigureAwait(false);
                    }
                    catch (SocketException)
                    {

                    }
                    catch (TimeoutException)
                    {

                    }
                    Console.WriteLine(stream.ToLog());
                });
            }

        }
        public static async Task RunProxySsl()
        {
            Console.WriteLine(Environment.StackTrace);
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 443);
            tcpListener.Start();

            while (true)
            {
                var client = await tcpListener.AcceptTcpClientAsync().ConfigureAwait(false);

                var t1 = Task.Run(async () =>
                {
                    Console.WriteLine($"New Client SSl");
                    var s = client.GetStream();
                    DebugStream stream = new DebugStream(s, ELogType.HexFormat);

                    TcpClient origin = new TcpClient();
                    await origin.ConnectAsync(adresses, 443);

                    var t2 = Task.Run(async () =>
                    {
                        byte[] buffer = new byte[4096];
                        var streamSend = origin.GetStream();
                        while (true)
                        {
                            int readed = await stream.ReadAsync(buffer);
                            if (readed > 0) await streamSend.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, readed));
                        }
                    });

                    var t3 = Task.Run(async () =>
                    {
                        byte[] buffer = new byte[4096];
                        var streamSend = stream;
                        var streamRead = origin.GetStream();
                        while (true)
                        {
                            int readed = await streamRead.ReadAsync(buffer);
                            if (readed > 0) await streamSend.WriteAsync(new ReadOnlyMemory<byte>(buffer, 0, readed));
                        }
                    });
                    try
                    {
                        await Task.WhenAll(t2, t3).ConfigureAwait(false);
                    }
                    catch (ZeroReadException)
                    {
                        return;
                    }
                    catch (ZeroWriteException)
                    {
                        return;
                    }
                });
                await t1;
            }
        }

        public static async Task UpgradeConnect()
        {
            var responseString =
                "HTTP/1.1 426 Upgrade Required\r\n" +
                "Upgrade: HTTP/2\r\n" +
                "Connection: Upgrade\r\n" +
                "Content-Length: 53\r\n" +
                "Content-Type: text/plain\r\n" +
                "\r\n" +
                "This service requires use of the HTTP/2 protocol.\r\n";
            var response = Encoding.UTF8.GetBytes(responseString);

            TcpListener listener = new TcpListener(IPAddress.Any, 60121);
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
                var stream = client.GetStream();

                Task t1 = Task.Run(async () =>
                {
                    byte[] buffer = new byte[(1 << 20) * 8];

                    while (true)
                    {
                        int readed = await stream.ReadAsync(buffer).ConfigureAwait(false);
                        if (readed > 0)
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(new ReadOnlySpan<byte>(buffer, 0, readed)));
                        }
                    }
                });
                //await Task.Delay(1000);
                //await stream.WriteAsync(HttpLibConstants.Responses.bytesOk11).ConfigureAwait(false);
                await Task.Delay(1000);
                await stream.WriteAsync(response).ConfigureAwait(false);
                Console.WriteLine("Response Sended");
                await t1;
            }
        }
    }
}
