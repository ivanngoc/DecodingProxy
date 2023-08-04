using System;

namespace IziHardGames.Libs.Networking.Clients
{
    public enum EConnectionState
    {
        None,
        Active,
        Stalled,
        Disposed,
    }

    [Flags]
    public enum EConnectionFlags
    {
        Reseted = -1,
        None = 0,
        Ssl,
        AuthenticatedSslClient,
        AuthenticatedSslServer,
        AuthenticatedConnection,
        TimeoutPresented,
        LifePresented,
    }
}