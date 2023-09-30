using System;

namespace IziHardGames.Proxy
{
    [Flags]
    public enum EHttpConnectionFlags
    {
        Error = -1,
        None,

        Closed = 1 << 0,
        Stalled = 1 << 1,
        Faulted = 1 << 2,
        // TLS
        AuthenticatedHttp11 = 1 << 3,
        AuthenticatedHttp20 = 1 << 4,
        AuthenticatedHttp30 = 1 << 5,

        HTTP11 = 1 << 6,
        HTTP20 = 1 << 7,
        HTTP30 = 1 << 8,
    }
}