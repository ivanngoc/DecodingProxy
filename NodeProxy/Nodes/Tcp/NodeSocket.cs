using System.Collections.Generic;
using System.Net.Sockets;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeSocket : Node
    {
        protected Socket socket;
        protected readonly Queue<DataFragment> fragments = new Queue<DataFragment>();
        protected readonly NodeConnectionControl control;

        public NodeSocket(NodeConnectionControl control) : base()
        {
            this.control = control;
        }
        internal void SetSocket(Socket socket)
        {
            this.socket = socket;
        }
    }
}
