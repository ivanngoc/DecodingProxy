using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Attributes;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Proxy.TcpDecoder;
using System;
using System.Buffers;

namespace IziHardGames.Libs.Cryptography.Tls.Shared
{
    public ref struct TlsHelloFromServerExtensionsEnumerator
    {
        public ReadOnlySequence<byte> data;
        public TlsExtension Current { get; set; }
        public bool IsError { get; set; }
        /// <summary>
        /// Current offset in <see cref="data"/>
        /// </summary>
        public int offset;
        private ushort lengthLeft;

        public TlsHelloFromServerExtensionsEnumerator(in ReadOnlyMemory<byte> mem) : this()
        {
            throw new System.NotImplementedException();
        }

        [HandshakeAnalyz(Side = EHandshakeSide.Server)]
        public TlsHelloFromServerExtensionsEnumerator(in ReadOnlySequence<byte> data) : this()
        {
            this.data = data;
            if (TlsHandshakeReadOperation.CheckIntegrity(in data, out ushort length))
            {
#if DEBUG
                //  Don't use bytes 3,4: Some TLS servers fail if the record version is greater than TLS 1.0 for the initial ClientHello.
                ushort protocolVersion = BufferReader.ToUshort(data.GetItemAt(9), data.GetItemAt(10));
                if (protocolVersion != ConstantsForTls.CLIENT_VERSION_TLS12) throw new ArgumentOutOfRangeException($"Expected TLS1.2 But recived: {protocolVersion}");
#endif
                if (data.IsSingleSegment)
                {
                    var span = data.FirstSpan;
                    byte sessionIDLength = span[43];
                    offset = 44 + sessionIDLength + 3;
                    lengthLeft = BufferReader.ToUshort(span[offset], span[offset + 1]);
                }
                else
                {
                    byte sessionIDLength = data.GetItemAt(43);
                    offset = 44 + sessionIDLength + 3;
                    lengthLeft = BufferReader.ToUshort(data.GetItemAt(offset), data.GetItemAt(offset + 1));
                }
                offset += 2;
            }
            else
            {
                throw new FormatException($"Tls Frame is Not Valid");
            }
        }
        public bool MoveNext()
        {
            if (IsError) return false;
            if (lengthLeft > 0)
            {
                if (data.IsSingleSegment)
                {
                    var span = data.FirstSpan;
                    ushort type = BufferReader.ToUshort(span[offset], span[offset + 1]);
                    offset += 2;
                    ushort length = BufferReader.ToUshort(span[offset], span[offset + 1]);
                    offset += 2;
                    Current = new TlsExtension(type: type, length: length, data: data.Slice(offset, length));
                    offset += length;
                    lengthLeft -= (ushort)(4 + length);
                    return true;
                }
                else
                {
                    var slice = data.Slice(offset);
                    Span<byte> span2 = stackalloc byte[2];
                    slice.CopyTo(span2);
                    ushort type = BufferReader.ToUshort(span2);
                    offset += 2;
                    data.Slice(offset).CopyTo(span2);
                    ushort length = BufferReader.ToUshort(span2);
                    offset += 2;
                    Current = new TlsExtension(type: type, length: length, data: data.Slice(offset, length));
                    offset += length;
                    lengthLeft -= (ushort)(4 + length);
                    return true;
                }
            }
            return false;
        }

      
    }
}