using System;

namespace IziHardGames.Libs.ForHttp.Monitoring
{
    [Flags]
    public enum EHttpConnectionStates
    {
        All = -1,
        None = 0,

        ClientConnected = 1 << 0,
        OriginConnected = 1 << 1,

        ClientAuthenticated = 1 << 2,
        OriginAuthenticated = 1 << 3,

        Stalled = 1 << 4,
        Disconnected = 1 << 5,
    }

}