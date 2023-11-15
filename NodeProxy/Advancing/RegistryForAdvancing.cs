using IziHardGames.NodeProxies.Nodes;
using IziHardGames.Graphs.Abstractions.Lib;
using IziHardGames.NodeProxies.Advancing;
using System;
using System.Collections.Generic;
using IziHardGames.ObjectPools.Abstractions.Lib.Abstractions;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using IziHardGames.NodeProxies.Nodes.SOCKS5;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<string, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using IziHardGames.NodeProxies.Nodes.Tls;

namespace IziHardGames.NodeProxies.Graphs
{
    internal sealed class RegistryForAdvancing : IIziNodeAdvancingSearcher
    {
        private readonly Dictionary<Node, NextNode> queue = new Dictionary<Node, NextNode>();

        internal AdvanceResult GetAdvancing(int variant, Node node, IziGraph graph)
        {
            var indx = graph!.indexators[typeof(Indx)].As<Indx>();

            if (queue.TryGetValue(node, out var nextNode))
            {
                AdvanceResult result = IziPool.GetConcurrent<AdvanceResult>();
                result.Add(nextNode);
                return result.AsVariant(-1);
            }

            if (node is NodeSocket nodeSocket)
            {
                if (variant == default)
                {
                    return NodeAdvanceAfterSocket.Advance(this, nodeSocket, graph).AsVariant(default);
                }
                throw new System.NotImplementedException();
            }
            else if (node is NodeGate gate)
            {
                if (variant == default)
                {
                    return NodeAdvanceAfterNodeGate.Advance(this, gate, graph).AsVariant(default);
                }
                throw new System.NotImplementedException();
            }
            else if (node is NodeSocksGreetAsServer socks5)
            {
                if (variant == default)
                {
                    AdvanceResult result = IziPool.GetConcurrent<AdvanceResult>();
                    NodeGate nodeGate = new NodeGate();
                    result.Add(new NextNode(nodeGate, ERelations.Next));
                    return result;
                }
            }
            else if (node is NodeTlsClientAuth tlsClient)
            {

            }
            else if (node is NodeTlsServerAuth tlsServ)
            {

            }
            else if (node is NodeSmartProxyTcp nodeSmartProxyTcp)
            {
                throw new System.NotImplementedException();
            }
            else
            {
                throw new System.NotImplementedException($"type:{node.GetType().FullName}");
            }
            throw new System.NotImplementedException();
        }

        internal RegistryForAdvancing QueueNode(Node a, Node b, ERelations relations)
        {
            queue.Add(a, new NextNode(b, relations));
            return this;
        }
        internal RegistryForAdvancing QueueNode<T>(Node node) where T : Node
        {
            throw new System.NotImplementedException();
        }
    }
}