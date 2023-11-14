using System;
using System.Buffers;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Binary.Writers;
using IziHardGames.Socks5.Enums;
using IziHardGames.Socks5.Headers;

namespace IziHardGames.Socks5
{
    public class ServerForSocks5
    {
        private static readonly IPAddress localhost = IPAddress.Parse("127.0.0.1");
        private static int count = default;
        private static bool isOverrideTarget = true;
        private const int mitmproxyPort = 63401;
        public static async Task Test()
        {
            int port = 44888;
            // 
            TcpListener listener = new TcpListener(System.Net.IPAddress.Any, port);
            listener.Start();

            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                client.ReceiveTimeout = 1000 * 60 * 15;
                client.SendTimeout = 1000 * 60 * 15;

                Console.WriteLine($"Client accepted");
                var t1 = Start(client);
                var t2 = t1.ContinueWith((x) => Finish(x, client));
                Interlocked.Increment(ref count);
            }
        }

        private static void Finish(Task t1, TcpClient client)
        {
            if (t1.IsFaulted)
            {
                Console.WriteLine($"Exception:\r\n{t1.Exception.Message}");
            }
            client.Close();
            Console.WriteLine($"Complete client accepted");
            Interlocked.Decrement(ref count);
        }

        private static Task Start(TcpClient client)
        {
            return Task.Run(async () => await HandleTcpClient(client));
        }

        private static async Task HandleTcpClient(TcpClient client)
        {
            var clientStream = client.GetStream();
            byte[] buffer = new byte[(1 << 20) * 4];


            int readed = await clientStream.ReadAsync(buffer);
            Console.WriteLine($"Readed: {readed}");
            var mem = new ReadOnlyMemory<byte>(buffer, 0, readed);
            var slice = mem;
            var greet = BufferReader.ToStructConsume<ClientGreetingsSocks5>(ref slice);
            Console.WriteLine(greet.ToStringInfo());
            byte[] methods = BufferReader.Consume(greet.numberOfAuthMethods, ref slice).ToArray();
            Console.WriteLine($"Auth methods:\t{methods.Select(x => (EAuth)x).Select(x => x.ToString()).Aggregate((x, y) => x + '\t' + y)}");

            if (methods[0] != (byte)EAuth.NoAuthRequired) throw new NotImplementedException();

            ServerChoice serverChoice = new ServerChoice();
            serverChoice.version = 5;
            serverChoice.cauth = methods[0]; // (byte)EAuth.None;
            int length = BufferWriter.WirteToBuffer(serverChoice, buffer, 0);
            await clientStream.WriteAsync(buffer, 0, length);

            readed = await clientStream.ReadAsync(buffer);
            if (readed == 0) throw new FormatException();

            mem = new ReadOnlyMemory<byte>(buffer, 0, readed);
            slice = mem;
            var ccr = BufferReader.ToStructConsume<ClientRequest>(ref slice);
            Console.WriteLine(ccr.ToStringInfo());
            var adr = ccr.atyp;

            IPAddress iPAddress = default;
            switch (adr.Type)
            {
                case EAdrType.None: throw new FormatException();
                case EAdrType.IPv4:
                    {
                        iPAddress = BufferReader.ConsumeIPv4AsIPAddress(ref slice);
                        break;
                    }
                case EAdrType.DomainName:
                    {
                        byte lengthDomainName = BufferReader.ConsumeByte(ref slice);
                        var domainName = BufferReader.Consume(lengthDomainName, ref slice);
                        var ips = Dns.GetHostAddresses(Encoding.UTF8.GetString(domainName.Span));
                        iPAddress = ips.First(x => x.AddressFamily == AddressFamily.InterNetwork);
                        if (iPAddress is null) throw new NotImplementedException();
                        break;
                    }
                case EAdrType.IPv6:
                    {
                        iPAddress = BufferReader.ConsumeIPv6AsIPAddress(ref slice);
                        break;
                    }
                default:
                    break;
            }

            ushort destinationPort = BufferReader.ToUshortConsume(ref slice);

            var auth = EAuth.NoAuthRequired;
            switch (auth)
            {
                case EAuth.NoAuthRequired:
                    break;
                case EAuth.GSSAPI:
                    readed = await clientStream.ReadAsync(buffer, 0, buffer.Length);
                    break;
                case EAuth.UserPassword:
                    break;
                case EAuth.Challenge:
                    break;
                case EAuth.Unassigned:
                    break;
                case EAuth.ChallengeResponseAuthenticationMethod:
                    break;
                case EAuth.SecureSocketsLayer:
                    break;
                case EAuth.NDSAuthentication:
                    break;
                case EAuth.MultiAuthenticationFramework:
                    break;
                case EAuth.JSONParameterBlock:
                    break;
                default: throw new System.NotImplementedException();
            }
            TcpClient dest = new TcpClient();
            if (isOverrideTarget)
            {
                await dest.ConnectAsync(localhost, mitmproxyPort).ConfigureAwait(false);
            }
            else
            {
                if (iPAddress!.ToString() == "0.0.0.0")
                {
                    Console.WriteLine("Detected IP:0.0.0.0; Replace with localhost");
                    iPAddress = localhost;
                }
                // System.Net.Sockets.SocketException: 'A socket operation was attempted to an unreachable network.'
                await dest.ConnectAsync(iPAddress!, destinationPort).ConfigureAwait(false);
            }


            length = default;
            ServerReply resp = new ServerReply();
            resp.VER = ESocksType.SOCKS5;
            resp.Reply = EReply.RequestGranted;
            resp.atyp.Type = iPAddress!.AddressFamily == AddressFamily.InterNetwork ? EAdrType.IPv4 : EAdrType.IPv6;

            // 4 bytes
            length += BufferWriter.WirteToBuffer(resp, buffer, 0);
            // var
            var adrBytes = iPAddress!.GetAddressBytes();
            length += BufferWriter.WirteToBuffer(adrBytes, buffer, length);
            // 2 byte
            length += BufferWriter.WirteToBufferUshort(destinationPort, buffer, length);

            Console.WriteLine(resp.ToStringInfo());
            await clientStream.WriteAsync(buffer, 0, length);

            var v = await ExchangeAsync(client, dest).ConfigureAwait(false);
            Console.WriteLine($"Complete exchange. ClientToServer:{v.Item1} bytes\t Server To CLient:{v.Item2} bytes");
        }

        private static async Task<(int, int)> ExchangeAsync(TcpClient client, TcpClient dest, CancellationToken ct = default)
        {
            CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var stream = client.GetStream();
            var destStr = dest.GetStream();
            ConnectionTunnel state = new ConnectionTunnel();
            state.Bind(client, dest);

            var t1 = Task.Run(async () =>
            {
                return await Copy(stream, destStr, state, true, cts.Token);
            });
            var t2 = Task.Run(async () =>
            {
                return await Copy(destStr, stream, state, false, cts.Token);
            });

            await Task.WhenAll(t1, t2).ConfigureAwait(false);
            cts.Cancel();
            return (t1.Result, t2.Result);
        }

        private static async ValueTask<int> Copy(NetworkStream from, NetworkStream to, ConnectionTunnel state, bool isDirect, CancellationToken ct = default)
        {
            int totalBytesTransfered = default;
            byte[] bufferRead = ArrayPool<byte>.Shared.Rent((1 << 10) * 512);

            while (!ct.IsCancellationRequested)
            {
                int readed = await from.ReadAsync(bufferRead, 0, bufferRead.Length);
                if (readed > 0)
                {
                    totalBytesTransfered += readed;
                    Console.WriteLine($"Transfered [{(isDirect ? "AB" : "BA")}]: {readed} bytes\t connections:{count}");
                    await to.WriteAsync(bufferRead, 0, readed);

                    if (isDirect) state.ZeroReadResetAB();
                    else state.ZeroReadResetBA();
                }
                else
                {
                    await Task.Delay(200);
                    Console.WriteLine($"Zero Read: IsDirect:{isDirect}. {state.ToStringInfo()}\t connections:{count}");
                    if (isDirect)
                    {
                        if (state.ZeroReadAB())
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (state.ZeroReadBA())
                        {
                            break;
                        }
                    }
                }
            }
            ArrayPool<byte>.Shared.Return(bufferRead);
            return totalBytesTransfered;
        }
    }
}
