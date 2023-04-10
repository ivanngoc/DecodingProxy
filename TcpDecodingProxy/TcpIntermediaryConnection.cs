using System.Net.Sockets;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TcpIntermediaryConnection
    {
        public const int DEFAULT_HISTORY_SIZE = Sizes.MiB * 128;
        private TcpClient client;
        private byte[] history;

        public TcpIntermediaryConnection(int historySize)
        {
            history = new byte[historySize];
        }
    }

    public class TcpMediator
    {
        public TcpIntermediaryConnection toAgent;
        public TcpIntermediaryConnection toOrigin;
    }
}