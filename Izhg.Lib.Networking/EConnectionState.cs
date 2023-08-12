using System;

namespace IziHardGames.Libs.Networking.States
{

    public enum EUpgradingType
    {
        None,
        /// <summary>
        /// Application Layer Protocol Negotiation
        /// </summary>
        ALPN,
        Direct,
        Upgrade,
        /// <summary>
        /// Next Protocol Negotiation
        /// </summary>
        NPN
    }

    public enum EConnectionState
    {
        None,
        Active,
        Stalled,
        Disposed,
    }

    [Flags]
    public enum EConnectionFlags : int
    {
        Reseted = -1,
        None = 0,
        Ssl,
        AuthenticatedSslClient,
        AuthenticatedSslServer,
        AuthenticatedConnection,
        TimeoutPresented,
        LifePresented,
        /// <summary>
        /// Connection support multiple protocols on the same port and server
        /// </summary>
        Multiprotocol,

        TLS1,
        TLS12,
        TLS13,

        HTTP1,
        HTTP11,
        HTTP2,
        HTTP3,
    }

    /// <summary>
    /// Используется для определения типа подключения когда соединение утсановлено и пошли первые сообщения
    /// </summary>
    public enum ENegotioanions
    {
        None,
        Connect,
        Direct,
        Handshake,
    }
}