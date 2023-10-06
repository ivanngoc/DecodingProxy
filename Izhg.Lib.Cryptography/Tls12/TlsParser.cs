using System;
using System.Security.Cryptography.X509Certificates;
using IziHardGames.Libs.Binary.Readers;
using static IziHardGames.Libs.Cryptography.Tls12.TlsConnection12;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    public static class TlsParser
    {
        public static X509Certificate2Collection ParseServerCert(in ParseResult handshakeRecord, in ReadOnlyMemory<byte> payload)
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