using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    /// <summary>
    /// Только у этого <see cref="SocketReader"/> есть прямой доступ на чтение из <see cref="Socket"/>
    /// </summary>
    public class SockerReaderRawStackable : SocketReader
    {
        private Socket? socket;
        private long totalBytes;
#if DEBUG
        private string debug = string.Empty;
#endif

        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            Initilize(wrap.Socket);
        }
        private void Initilize(Socket socket)
        {
            this.socket = socket;
        }
        public override void Dispose()
        {
            base.Dispose();
            socket = default;
            totalBytes = default;
        }
        public async override ValueTask<int> TransferToAsync(Memory<byte> mem, CancellationToken ct = default)
        {
#if DEBUG
            if (mem.Length <= 0) throw new ArgumentException($"Buffer length must be greater than 0");
#endif
            while (true)
            {
                var recived = await socket!.ReceiveAsync(mem, SocketFlags.None, ct).ConfigureAwait(false);
                if (recived > 0)
                {
#if DEBUG
                    debug += Encoding.UTF8.GetString(mem.Slice(0, recived).Span);
                    Console.WriteLine($"RawRaeder [{GetHashCode()}]: " + ParseByte.ToHexStringFormated(mem.Slice(0, recived)));
#endif
                    totalBytes += recived;
                    foreach (var interceptor in interceptorsIn)
                    {
                        var status = interceptor.Intercept(in mem);
                    }
                    return recived;
                }
                else
                {
                    await Task.Delay(100).ConfigureAwait(false);
                }
            }
        }

        public override int TransferTo(byte[] array, int offset, int length)
        {
            return socket!.Receive(array, offset, length, SocketFlags.None);
        }
    }
}