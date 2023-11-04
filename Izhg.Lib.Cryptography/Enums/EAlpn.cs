using System;

namespace IziHardGames.Libs.Cryptography
{
    [Flags]
    public enum EAlpn
    {
        Unknown = 0,
        None = 1 << 0,
        h3 = 1 << 1,
        h2 = 1 << 2,
        http11 = 1 << 3,
    }
}
