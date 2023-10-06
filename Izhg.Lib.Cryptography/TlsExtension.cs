using System;
using System.Buffers;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Tls;

namespace IziHardGames.Proxy.TcpDecoder
{
    public readonly ref struct TlsExtension
    {
        public readonly ushort type;
        public readonly ushort length;
        public readonly ReadOnlySequence<byte> data;
        public ETlsExtensions Type => (ETlsExtensions)BufferReader.ReverseEndians(type);

        public TlsExtension(ushort type, ushort length, in ReadOnlySequence<byte> data) : this()
        {
            this.type = type;
            this.length = length;
            this.data = data;
        }

        public bool Validate()
        {
            return true;
        }

        public static TlsExtension ReadConsume(ref ReadOnlyMemory<byte> mem)
        {
            throw new System.NotImplementedException();
        }

        public static bool TryReadConsume(ref ReadOnlyMemory<byte> mem, out TlsExtension extension)
        {
            if (mem.Length > 4)
            {
                var span = mem.Span;
                ushort type = BufferReader.ToUshortConsume(ref span);
                ushort length = BufferReader.ToUshortConsume(ref span);
                var payload = mem.Slice(4, length);
                var seq = new ReadOnlySequence<byte>(payload);
                extension = new TlsExtension(type: type, length: length, data: in seq);
                mem = mem.Slice(4 + length);
                return extension.Validate();
            }
            extension = default;
            return false;
        }

        public string ToStringInfo()
        {
            return $"Type:{Type}; Length:{length}; Actual Lenth:{data.Length}";
        }
    }
}