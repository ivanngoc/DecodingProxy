using System.Net;

namespace IziHardGames.NodeProxies.Advancing
{
    public static class ConstantsForNodeProxy
    {
        public const string INDX_AGENT_CONN_CONTROL = "AgentConnectionControl";
        public const string INDX_AGENT_SOCKET_READER = "AgentSocketReader";
        public const string INDX_AGENT_SOCKET_WRITER = "AgentSocketWriter";
        public const string INDX_GATE = "NODE_GATE";
        public const string INDX_SOCKS_GREET = nameof(INDX_SOCKS_GREET);

        public const string INDX_ORIGIN_SOCKET = nameof(INDX_ORIGIN_SOCKET);
        public const string INDX_ORIGIN_CONN_CONTROL = nameof(INDX_ORIGIN_CONN_CONTROL);
        public const string INDX_ORIGIN_SOCKET_READER = nameof(INDX_ORIGIN_SOCKET_READER);
        public const string INDX_ORIGIN_SOCKET_WRITER = nameof(INDX_ORIGIN_SOCKET_WRITER);
        
        
    }

    public static class NodeProxyGlobals
    {
        private static bool isOverrideSocks5Endpoint = true;
        private static IPAddress addressSocks5 = IPAddress.Parse("127.0.0.1");
        private static ushort portSocks5 = 43601;
        public static IPAddress GetDestAddressSocks5(IPAddress address)
        {
            if (isOverrideSocks5Endpoint) return addressSocks5;
            else return address;
        }
        public static ushort GetDestPortSocks5(ushort port)
        {
            if (isOverrideSocks5Endpoint) return port;
            return portSocks5;

        }
    }
}
