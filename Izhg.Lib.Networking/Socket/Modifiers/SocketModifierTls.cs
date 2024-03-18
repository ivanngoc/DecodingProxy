using System;
using System.Net.Security;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public abstract class SocketModifierTls : SocketWrapModifier
    {
        private readonly SslStreamReader reader = new SslStreamReader();

        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            reader.Initilize(wrap);
        }
        public override void InitilizeReverse()
        {
            base.InitilizeReverse();
        }
        public void SetStream(SslStream stream)
        {
            reader.SetStream(stream);
        }
        public override void Dispose()
        {
            base.Dispose();
            reader.Dispose();
        }
    }

    public class SslStreamReader : SocketReader
    {
        private SslStream? sslStream;
        public async override ValueTask<int> TransferToAsync(Memory<byte> mem, CancellationToken ct = default)
        {
            var readed = await sslStream!.ReadAsync(mem, ct).ConfigureAwait(false);
            return readed;
        }
        public override int TransferTo(byte[] array, int offset, int length)
        {
            return sslStream!.Read(array, offset, length);
        }
        public void SetStream(SslStream stream)
        {
            this.sslStream = stream;
        }
        public override void Dispose()
        {
            base.Dispose();
            sslStream!.Dispose();
        }
    }
}