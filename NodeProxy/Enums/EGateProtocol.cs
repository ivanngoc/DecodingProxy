namespace IziHardGames.NodeProxies.Nodes
{
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
