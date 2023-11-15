#pragma warning disable

using System;
using System.Net;
using System.Threading.Tasks;
using IziHardGames.MappedFrameReader;
using IziHardGames.Libs.ForHttp20;
using Test;
using IziHardGames.Libs.HttpCommon.Recording;
using System.IO;
using System.Text;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Libs.Binary.Readers;
using DevConsole.Shared.Consoles;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Tls;
using IziHardGames.Socks5;
using IziHardGames.Proxy.Http;
using IziHardGames.Libs.Concurrency;
using IziHardGames.Libs.HttpCommon.Common;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Threading;
using IziHardGames.Libs.HttpCommon;
using IziHardGames.Proxy.Recoreder;
using IziHardGames.DataRecording.Abstractions.Lib;

namespace IziHardGames.Tests
{
    public static partial class Program
    {
        private static readonly object locker = new object();
        public static async Task Main(params string[] args)
        {
            Console.WriteLine("Begin Test");
            //IziDataRecorder.ToDataBase = ;

            if (false)
            {
                await ServerForSocks5.Test();
                Console.WriteLine();
            }
            if (false)
            {
                ConsoleClient consoleClient = new ConsoleClient();
                var logger = await consoleClient.Start();
                logger.WriteLine($"Waiting t1...");
                logger.WriteLine($"Complete init");
                int i = default;
                Console.ReadLine();
                while (true)
                {
                    logger.WriteLine($"{DateTime.Now}  Client1 {i++}");
                    await Task.Delay(500);
                }
            }

            if (false)
            {
                if (false)
                {
                    DevConsoleControl devConsoleControl = new DevConsoleControl();
                    await devConsoleControl.Start().ConfigureAwait(false);
                    ReportPublisher.ReportFunc = devConsoleControl.SendToServer;
                }
                HttpRecordAnalyzer analyzer = new HttpRecordAnalyzer();
                await analyzer.Run().ConfigureAwait(false);
            }

            if (false)
            {
                SchemeImporter importer = new SchemeImporter();
                importer.tableOfFuncs.AddAdvancingFunc($"ReadBodyHttp11", PopularFuncs.ReadBodyHttp11);

                Scheme scheme = await importer.FromFileAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\Izhg.MappedFrameReader\\Examples\\SchemeSsl.txt");
                byte[] testData = await File.ReadAllBytesAsync("C:\\Users\\ngoc\\Documents\\[Projects] C#\\IziHardGamesProxy\\Izhg.MappedFrameReader\\Test\\1 8e571345-6153-46e4-80ca-386f673073f6.reader.clear");

                Reader reader = new Reader(scheme);
                reader.scheme.RegistHandlers("HttpConnect.StartLine",
                    (x) =>
                        {
                            Yellow(Encoding.UTF8.GetString(x.Span));
                            return 0;
                        }
                    );
                reader.scheme.RegistHandlers("HttpConnect", (x) =>
                {
                    Yellow(Encoding.UTF8.GetString(x.Span)); return 0;
                });
                reader.scheme.RegistHandlers("HttpConnect.Headers", (x) => { Yellow(Encoding.UTF8.GetString(x.Span)); return 0; });
                reader.scheme.RegistHandlers("SslFrame", (x) =>
                {
                    Yellow(Encoding.UTF8.GetString(x.Span)); return 0;
                });
                reader.scheme.RegistHandlers("SslFrame.TlsRecord.RecordType", (x) =>
                {
                    Yellow($"{TlsRecord.GetType(in x)}");
                    return 0;
                });
                reader.scheme.RegistHandlers("SslFrame.TlsRecord.ProtocolVersion", (x) => { Yellow($"{ProtocolVersion.GetStringInfo(in x)}"); return 0; });
                reader.scheme.RegistHandlers("SslFrame.TlsRecord", (x) => { Yellow($"{TlsRecord.GetStringInfo(in x)}"); return 0; });
                reader.scheme.RegistHandlers("SslFrame.TlsRecord.Length", (x) => { Yellow($"{BufferReader.ToUshort(x.Span)}"); return 0; });
                reader.scheme.RegistHandlers("SslFrame.TlsRecord.Payload", (x) => { Yellow($"{ParseByte.ToHexStringFormated(in x)}"); return 0; });
                reader.scheme.RegistHandlers("SslFrame.TlsRecord.Payload.HandshakeHeader", (x) => { Yellow($"{HandshakeHeader.GetStringInfo(in x)}"); return 0; });

                await reader.ReadAllAsync(testData);
            }


            if (false)
            {
                //await SchemeImporter.Test();
                Console.WriteLine($"Scheme Imported");
                Console.ReadLine();
                await TestQuic.Test();
                //await AsyncFlowControl.Test();
                ReaderHttp2.Test();
                //TlcHelloFromClient.Test12();
                //Console.ReadLine();
                //await TestHttp2.RunTlsParse();
                //await TestHttp2.ShowIp();
                await TestHttp2.UpgradeConnect2();
                //await TestHttp2.RunProxy();
                //await TestHttp2.RunProxySsl();
                //await TestHttp2.RunHttp2Ssl();
                //await TestHttp2.RunHttp2();
                await DoAsync();
            }
            if (true)
            {
                await RunHttps(63401);
            }
        }



        private static async Task DoAsync()
        {
            var adressed = await Dns.GetHostAddressesAsync("zeroscans.com");

            foreach (var item in adressed)
            {
                Console.WriteLine(item.ToString());
            }

            Console.WriteLine($"Begin");

            //var t1 = TcpInfo.Run(80);
            //var t1 = TcpInfo.RunHex(443);
            var t1 = TcpInfo.Run(49702);
            //var t1 = TcpInfo.Run(60122);
            //var t1 = TcpInfo.RunUdp(60122);
            //var t2 = TcpInfo.Run(61255);
            await Task.WhenAll(t1);
            //await Task.WhenAll(t1,t2);
        }

        static void Log(string msg, ConsoleColor color)
        {
            var back = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.Now.Ticks}\t{msg}");
            Console.ForegroundColor = back;
        }
        static void Yellow(string msg)
        {
            Log(msg, ConsoleColor.Yellow);
        }
        static void Green(string msg)
        {
            Log(msg, ConsoleColor.Green);
        }

        private static async Task RunHttps(int port, CancellationToken ct = default)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            HttpConsumer httpConsumer = new HttpConsumer();
            httpConsumer.SetManager(new ManagerForHttpSessionDefault());

            while (!ct.IsCancellationRequested)
            {
                var socket = await listener.AcceptSocketAsync(ct).ConfigureAwait(false);
                Task task = HttpProxyProcessor.HandleSocket(httpConsumer, socket);
            }
        }
    }
}