using System;

namespace IziHardGames.Libs.Cryptography
{
    public class TlsFlags
    {
        public static EAlpn GetAlpnProtocol(in ReadOnlyMemory<byte> data)
        {
            if (data.CompareWith(ConstantsForTls.ALPN.h3))
            {
                return EAlpn.h3;
            }
            else if (data.CompareWith(ConstantsForTls.ALPN.h2))
            {
                return EAlpn.h2;
            }
            else if (data.CompareWith(ConstantsForTls.ALPN.http11))
            {
                return EAlpn.http11;
            }
            else
            {
                return EAlpn.Unknown;
            }
        }
    }
}
