using System.Runtime.InteropServices;
using IziHardGames.Graphs.Abstractions.Lib;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using IziHardGames.NodeProxies.Graphs;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.NodeProxies.Nodes.SOCKS5;
using IziHardGames.NodeProxies.Nodes.Tls;
using IziHardGames.ObjectPools.Abstractions.Lib.Abstractions;

namespace IziHardGames.NodeProxies.Advancing
{
    internal class NodeAdvanceAfterNodeGate : IIziNodeAdvancingAdapter
    {
        internal static AdvanceResult Advance(RegistryForAdvancing registry, NodeGate nodeGate, IziGraph graph)
        {
            AdvanceResult result = IziPool.GetConcurrent<AdvanceResult>();
            var prot = nodeGate.Protocol;
            if (prot == EGateProtocol.SOCKS5)
            {
                NodeSocksGreetAsServer node = IziPool.GetConcurrent<NodeSocksGreetAsServer>();
                node.SetGate(nodeGate);
                result.Add(new NextNode(node, ERelations.Next));

                NodeSocketOrigin nodeSocketOrigin = IziPool.GetConcurrent<NodeSocketOrigin>();
                result.Add(new NextNode(nodeSocketOrigin, ERelations.Next));
                node.SetOrigin(nodeSocketOrigin);
            }
            else if (prot == EGateProtocol.TLS_CLIENT)
            {
                NodeTlsClientAuth node = new NodeTlsClientAuth();
                result.Add(new NextNode(node, ERelations.Next));
            }
            else if (prot == EGateProtocol.TLS_SERVER)
            {
                NodeTlsServerAuth node = new NodeTlsServerAuth();
                result.Add(new NextNode(node, ERelations.Next));
            }
            else
            {
                throw new System.NotImplementedException();
            }
            return result;
        }
    }
}
