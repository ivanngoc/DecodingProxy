using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.ForHttp20;

namespace Test
{

    internal class TestHttp2
    {
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
    }
}
