using System.Runtime.InteropServices;
using IziHardGames.Graphs.Abstractions.Lib;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using IziHardGames.NodeProxies.Graphs;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.NodeProxies.Nodes.SOCKS5;
using IziHardGames.NodeProxies.Nodes.Tls;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using System;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.NodeProxies.Advancing
{
    internal class NodeAdvanceAfterNodeGate : IIziNodeAdvancingAdapter
    {
        internal static AdvanceResult Advance(RegistryForAdvancing registry, NodeGate nodeGate, IziGraph graph)
        {
            var indx = graph.indexators[typeof(Indx)].As<Indx>();
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
                indx[INDX_ORIGIN_SOCKET] = nodeSocketOrigin;
            }
            else if (prot == EGateProtocol.TLS_CLIENT)
            {
                NodeTlsAuthClient node = IziPool.GetConcurrent<NodeTlsAuthClient>();
                result.Add(new NextNode(node, ERelations.Next));
            }
            else if (prot == EGateProtocol.TLS_SERVER)
            {
                NodeTlsAuthServer node = IziPool.GetConcurrent<NodeTlsAuthServer>();
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
