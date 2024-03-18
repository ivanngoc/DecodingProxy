using System.Net;

namespace IziHardGames.NodeProxies.Advancing
{
    public static class ConstantsForNodeProxy
    {
        public const int INDX_GATE_AGENT = 0;
        public const int INDX_GATE_ORIGIN = 1;


        public const int INDX_AGENT_CONN_CONTROL = 2;
        public const int INDX_AGENT_SOCKET_READER = 3;
        public const int INDX_AGENT_SOCKET_WRITER = 4;

        public const int INDX_SOCKS_GREET = 5;

        public const int INDX_ORIGIN_SOCKET = 6;
        public const int INDX_CLIENT_SOCKET = 7;

        public const int INDX_ORIGIN_CONN_CONTROL = 8;
        public const int INDX_ORIGIN_SOCKET_READER = 9;
        public const int INDX_ORIGIN_SOCKET_WRITER = 10;


        public const int INDX_TLS_SERVER_READER = 11;
        public const int INDX_TLS_SERVER_WRITER = 12;
        public const int INDX_TLS_CLIENT_READER = 13;
        public const int INDX_TLS_CLIENT_WRITER = 14;

        public const int INDX_TLS_CLIENT_HUB = 15;
        public const int INDX_TLS_SERVER_HUB = 16;

        public const int INDX_SOCKET_BRIDGE = 17;
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
