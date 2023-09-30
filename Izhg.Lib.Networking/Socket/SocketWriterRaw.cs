using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public class SocketWriterRaw : SocketWriter
    {
        private Socket? socket;
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
        }

        public override int Write(byte[] array, int offset, int length)
        {
#if DEBUG
            Console.WriteLine($"WriterRaw [{GetHashCode()}]: {ParseByte.ToHexStringFormated(array, offset, length)}");
#endif
            return socket!.Send(array, offset, length,SocketFlags.None);
        }
        public async override Task WriteAsync(byte[] array, CancellationToken token = default)
        {
#if DEBUG
            Console.WriteLine($"WriterRaw [{GetHashCode()}]: {ParseByte.ToHexStringFormated(array, 0, array.Length)}");
#endif
            await socket!.SendAsync(array, SocketFlags.None, token).ConfigureAwait(false);
        }
        public async override Task WriteAsync(ReadOnlyMemory<byte> readOnlyMemory, CancellationToken token = default)
        {
#if DEBUG
            Console.WriteLine($"WriterRaw [{GetHashCode()}]: {ParseByte.ToHexStringFormated(readOnlyMemory)}");
#endif
            await socket!.SendAsync(readOnlyMemory, SocketFlags.None, token).ConfigureAwait(false);
        }
    }
}