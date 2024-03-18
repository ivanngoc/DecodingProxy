namespace IziHardGames.NodeProxies.Nodes
{
    internal enum EFrameType
    {
        None,
        Unknown,
        Tls,
        HTTP11,
        HTTP20,
    }

    internal enum EGateProtocol
    {
        None,
        TooShortToIdentify,
        Unknown,
        HTTP11,
        HTTP_CONNECT,
        SOCKS4,
        SOCKS5,
        TLS_SERVER,
        TLS_CLIENT,
        TLS_ERROR,
    }
}
