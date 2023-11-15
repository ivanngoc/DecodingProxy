using System.Collections.Generic;
using System.Net.Sockets;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeSocket : Node
    {
        protected Socket? socket;
        public Socket Socket => socket!;
        internal void SetSocket(Socket socket)
        {
            this.socket = socket;
        }
    }
}
