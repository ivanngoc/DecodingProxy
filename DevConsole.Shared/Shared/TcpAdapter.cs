using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevConsole.Shared.Consoles;
using IziHardGames.Libs.Streams;

namespace DevConsole.Shared
{
    public class TcpAdapter : CommunicationAdapter, IDisposable
    {
        private TcpClient tcpCLient;
        private NetworkStream stream;
        private StreamForLines textReader;

        public TcpAdapter(TcpClient tcpCLient) : base()
        {
            this.tcpCLient = tcpCLient;
            this.stream = tcpCLient.GetStream();
            this.textReader = new StreamForLines(this.stream);
        }
        internal async static Task<TcpClient> ConnectAndAuthorizeAsync(Encoding encoding, CancellationToken token = default)
        {
            TcpClient tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(ConstantsForConsoles.local, ConstantsForConsoles.SERVER_PORT, token).ConfigureAwait(false);
            await AuthorizeAsClientAsync(tcpClient, encoding, token).ConfigureAwait(false);
            return tcpClient;
        }
        public async static Task AuthorizeAsClientAsync(TcpClient tcpClient, Encoding encoding, CancellationToken token = default)
        {
            var stream = tcpClient.GetStream();

            string id = Guid.NewGuid().ToString();
            Header header = new Header()
            {
                magicNumber = ConstantsForConsoles.MAGIC_NUMBER_HEADER,
                length = (ushort)id.Length,
                type = ConstantsForConsoles.TYPE_ID,
            };
            string initMsg = $"{header.ToString()}\t{id}\r\n";
            Console.WriteLine($"Init msg Created:{initMsg}");
            await stream.WriteAsync(encoding.GetBytes(initMsg), token);
            Console.WriteLine($"Init msg Sended:{initMsg}");
        }
        public async override Task<string> Initilize()
        {
            while (true)
            {
                string header = await ReadLineAsync().ConfigureAwait(false);
                var splits = header.Split('\t');
                if (splits.Length == 4)
                {
                    int magicNumber = int.Parse(splits[0]);
                    ushort length = ushort.Parse(splits[1]);
                    int type = ushort.Parse(splits[2]);
                    string value = splits[3];
                    if (magicNumber == ConstantsForConsoles.MAGIC_NUMBER_HEADER)
                    {
                        return value;
                    }
                }
                else
                {
                    await Task.Delay(500);
                }
                Console.WriteLine($"Skipped:{header}");
            }
        }
        public async override Task<string> ReadLineAsync(CancellationToken ct = default)
        {
            var result = await textReader.ReadLineAsync(ct).ConfigureAwait(false);
            return result;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public override Task<LogHeader> ReadHeaderAsync()
        {
            throw new NotImplementedException();
        }
    }
}