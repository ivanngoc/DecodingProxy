using System;

namespace IziHardGames.Libs.Cryptography
{
    [Flags]
    public enum ESide
    {
        All = -1,
        None = 0,
        Server = 1 << 0,
        Client = 1 << 1,
    }
}
