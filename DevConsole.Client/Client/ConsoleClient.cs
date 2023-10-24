using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevConsole.Client.CompareWindow;
using DevConsole.Shared.Loggers;
using IziHardGames.Libs.Streams;

namespace DevConsole.Shared.Consoles
{
    public class ConsoleClient : IDisposable
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private Encoding encoding = Encoding.UTF8;
        private DLog logger;

        public ConsoleClient()
        {
            this.logger = new DLog(this);
        }

        public async Task RedirectV2()
        {

        }

        private async Task AuthorizeAsync(Stream stream, CancellationToken token = default)
        {
            await TcpAdapter.AuthorizeAsClientAsync(tcpClient, encoding, token).ConfigureAwait(false);
        }
        private async Task AuthorizeAsync(StreamForLines writer)
        {

            string id = Guid.NewGuid().ToString();
            Header header = new Header()
            {
                magicNumber = ConstantsForConsoles.MAGIC_NUMBER_HEADER,
                length = (ushort)id.Length,
                type = ConstantsForConsoles.TYPE_ID,
            };
            string initMsg = $"{header.ToString()}\t{id}";
            Console.WriteLine($"Init msg Created:{initMsg}");
            await writer.WriteLineAsync(initMsg);
            Console.WriteLine($"Init msg Sended:{initMsg}");
        }
        private async Task ConnectAsync()
        {
            this.tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ConstantsForConsoles.local, ConstantsForConsoles.SERVER_PORT).ConfigureAwait(false);
            Console.WriteLine($"TcpClient connected");
            this.stream = this.tcpClient.GetStream();

        }
        internal async Task RedirectOutputAsync(CancellationToken token = default)
        {
            Console.InputEncoding = encoding;
            Console.OutputEncoding = encoding;

            Console.WriteLine($"Begin redirect output");
            await ConnectAsync().ConfigureAwait(false);
            await AuthorizeAsync(this.stream, token).ConfigureAwait(false);
            Console.WriteLine($"Auth complete");
            MyTextWriter textWriter = new MyTextWriter(stream);
            Console.SetOut(textWriter);
            Console.WriteLine($"Set Out Complete");
        }
        internal async Task RedirectInputAsync()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            await ConnectAsync().ConfigureAwait(false);
            StreamForLines writer = new StreamForLines(stream);
            await AuthorizeAsync(writer);

            var stdIn = Console.OpenStandardInput();

            StreamForLines reader = new StreamForLines(stdIn);
            //var stdErr = Console.OpenStandardError();
            Console.WriteLine($"TcpClient created");

            var t1 = Task.CompletedTask;

            while (true)
            {
                string? line = await reader.ReadLineAsync().ConfigureAwait(false);
                await t1.ConfigureAwait(false);
                if (string.IsNullOrEmpty(line))
                {
                    t1 = Task.CompletedTask;
                }
                else
                {
                    Console.WriteLine($"Sended:{line}");
                    t1 = writer.WriteLineAsync(line);
                }
            }
        }

        private async Task SendRaw()
        {
            //while (true)
            //{
            //    int readed = await stdOut.ReadAsync(buffer).ConfigureAwait(false);
            //    await t1.ConfigureAwait(false);
            //    if (readed > 0)
            //    {
            //        t1 = stream.WriteAsync(buffer, 0, readed);
            //    }
            //    else
            //    {
            //        t1 = Task.CompletedTask;
            //    }
            //}
        }

        public async Task Stop()
        {
            await logger.Stop();
        }
        public async Task<DLog> Start()
        {
            var t1 = await logger.Start();
            return logger;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<CompareWindowClient> NewCompareWindowAsync()
        {
            CompareWindowClient compareWindowUnit = await CompareWindowClient.CreateAsync().ConfigureAwait(false);
            return compareWindowUnit.control;
        }
    }
}