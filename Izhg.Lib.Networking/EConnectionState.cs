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
        Ssl = 1 << 0,
        AuthenticatedSslClient = 1 << 1,
        AuthenticatedSslServer = 1 << 2,
        AuthenticatedConnection = 1 << 3,
        TimeoutPresented = 1 << 4,
        LifePresented = 1 << 5,
        /// <summary>
        /// Connection support multiple protocols on the same port and server
        /// </summary>
        Multiprotocol = 1 << 6,

        TLS1 = 1 << 7,
        TLS12 = 1 << 8,
        TLS13 = 1 << 9,

        HTTP1 = 1 << 10,
        HTTP11 = 1 << 11,
        HTTP2 = 1 << 12,
        HTTP3 = 1 << 13,
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

    [Flags]
    public enum ENetworkProtocols
    {
        All = -1,
        None = 0,

        HTTP1 = 1 << 1,
        HTTP11 = 1 << 2,
        HTTP2 = 1 << 3,
        HTTP3 = 1 << 4,

        Tls = 1 << 5,
        Tls11 = 1 << 6,
        Tls12 = 1 << 7,
        Tls13 = 1 << 8,
    }
}