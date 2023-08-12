using System.Buffers;

namespace IziHardGames.Proxy.TcpDecoder
{
    public readonly ref struct TlsExtension
    {
        public readonly ushort type;
        public readonly ushort length;
        public readonly ReadOnlySequence<byte> data;

        public TlsExtension(ushort type, ushort length, ReadOnlySequence<byte> data) : this()
        {
            this.type = type;
            this.length = length;
            this.data = data;
        }
    }
}