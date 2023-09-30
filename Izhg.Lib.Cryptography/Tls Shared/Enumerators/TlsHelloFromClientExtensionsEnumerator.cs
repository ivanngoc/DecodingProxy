using System;
using System.Buffers;
using IziHardGames.Core.Buffers;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Tls12;
using IziHardGames.Proxy.TcpDecoder;
using ByteEnum = IziHardGames.Libs.Buffers.Enumerators.EnumerotorForReadOnlySequence;

namespace IziHardGames.Libs.Cryptography.Tls.Shared
{
    public ref struct TlsHelloFromClientExtensionsEnumerator
    {
        public ReadOnlySequence<byte> data;
        public TlsExtension Current { get; set; }
        public bool IsError { get; set; }
        /// <summary>
        /// Current offset in <see cref="data"/>
        /// </summary>
        public int offset;
        private ushort lengthLeft;

        public TlsHelloFromClientExtensionsEnumerator(in ReadOnlySequence<byte> data) : this()
        {
            this.data = data;
#if DEBUG
            if (data.Length == 0) throw new ArgumentException($"Zero length sequence");
            //  Don't use bytes 3,4: Some TLS servers fail if the record version is greater than TLS 1.0 for the initial ClientHello.
            ushort protocolVersion = BufferReader.ToUshort(data.GetItemAt(9), data.GetItemAt(10));
            if (protocolVersion != ConstantsTls.CLIENT_VERSION_TLS12) throw new ArgumentOutOfRangeException($"Expected TLS1.2 But recived: {protocolVersion}");
#endif
            if (data.IsSingleSegment)
            {
                var span = data.FirstSpan;
                if (span.Length > 4)
                {
                    ushort msgSize = BufferReader.ToUshort(span[3], span[4]);
                    if (span.Length - 5 < msgSize) IsError = true;
                    else
                    {
                        byte sessionIdLength = span[43];
                        offset = 44;
                        offset += sessionIdLength;
                        ushort lengthCipherSuiteData = BufferReader.ToUshort(span[offset], span[offset + 1]);
                        offset += 2;
                        offset += lengthCipherSuiteData;
                        byte lengthCompressionMethodsData = span[offset];
                        offset++;
                        offset += lengthCompressionMethodsData;
                        lengthLeft = BufferReader.ToUshort(span[offset], span[offset + 1]);
                        offset += 2;
                    }
                }
            }
            else
            {
                if (data.Length < 46) IsError = true;
                ByteEnum num = new ByteEnum(data);
                var span = data.FirstSpan;
                if (span.Length < 5) throw new NotImplementedException();
                ushort msgSize = BufferReader.ToUshort(span[3], span[4]);
                if (data.Length - 5 < msgSize) IsError = true;
                else
                {
                    unsafe
                    {
                        Span<byte> span2b = stackalloc byte[2];
                        byte sessionIdLength = data.GetItemAt(43);
                        offset = 44;
                        offset += sessionIdLength;
                        var slice = data.Slice(offset);
                        slice.CopyTo(span2b);
                        ushort lengthCipherSuiteData = BufferReader.ToUshort(span2b);
                        offset += 2;
                        offset += lengthCipherSuiteData;
                        byte lengthCompressionMethodsData = data.GetItemAt(offset);
                        offset++;
                        offset += lengthCompressionMethodsData;
                        data.Slice(offset).CopyTo(span2b);
                        lengthLeft = BufferReader.ToUshort(span2b);
                        offset += 2;
                    }
                }
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