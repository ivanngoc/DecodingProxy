using System;
using System.Security.Authentication;

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
                case ConstantsTls.CLIENT_VERSION11: { return SslProtocols.Tls11; }
                case ConstantsTls.CLIENT_VERSION12: { return SslProtocols.Tls12; }
                case ConstantsTls.CLIENT_VERSION30: { return SslProtocols.Tls13; }
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