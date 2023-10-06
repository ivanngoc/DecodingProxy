using System;
using System.Security.Authentication;
using IziHardGames.Libs.Cryptography;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TlsParser
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
    }

    public enum EContentType : byte
    {
        None = 0,
        Handshake = 0x16,
    }
}