using System;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Libs.Binary.Readers;
using static IziHardGames.Libs.Cryptography.Readers.Tls12.ParserForTls12;

namespace IziHardGames.Libs.Cryptography.Readers
{
    public static class TlsParser
    {
        public static SslProtocols GetType(byte value)
        {
            throw new System.NotImplementedException();
        }
        public static SslProtocols GetProtocolVersion(ushort value)
        {
            switch (value)
            {
                case ConstantsForTls.CLIENT_VERSION_TLS11: { return SslProtocols.Tls11; }
                case ConstantsForTls.CLIENT_VERSION_TLS12: { return SslProtocols.Tls12; }
                case ConstantsForTls.CLIENT_VERSION_TLS13: { return SslProtocols.Tls13; }
                default: throw new ArgumentOutOfRangeException(value.ToString());
            }
        }
        public static X509Certificate2Collection ParseServerCert(in FrameParseResult handshakeRecord, in ReadOnlyMemory<byte> payload)
        {
            if (!handshakeRecord.handsakeHeader.ValidateAsServerCertificate()) throw new ArgumentException("Header is not certificate");
            X509Certificate2Collection result = new X509Certificate2Collection();

            var mem = payload;
            var lengthCerts = BufferReader.ToInt32Size3(mem.Span);
            mem = mem.Slice(3);

            while (lengthCerts > 0)
            {
                int length = BufferReader.ToInt32Size3(mem.Span);
                mem = mem.Slice(3);
                lengthCerts -= length;
                var certPayload = mem.Slice(0, length);
                result.Add(new X509Certificate2(certPayload.Span));
            }
            return result;
        }
    }
}