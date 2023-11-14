using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using IziHardGames.NodeProxies.Nodes;

namespace IziHardGames.NodeProxies.Graphs
{
    public sealed class ProxyGraph
    {
        private Node startNode;
        public Task RunAsync(Socket socket)
        {
            NodeSmartProxyTcp node = new NodeSmartProxyTcp();
            node.SetSocket(socket);
            node.ContinuationSetSmart();
            startNode = node;
            return Task.Run(async () => await node.ExecuteAsync());
        }
        public Task RunAsync(TcpClient tcpClient)
        {
            throw new System.NotImplementedException();
        }
        internal static ProxyGraph GetNew()
        {
            throw new NotImplementedException();
        }
    }
}