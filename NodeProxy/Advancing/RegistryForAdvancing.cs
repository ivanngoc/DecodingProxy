using IziHardGames.NodeProxies.Nodes;
using IziHardGames.Graphs.Abstractions.Lib;
using IziHardGames.NodeProxies.Advancing;
using System;
using System.Collections.Generic;
using IziHardGames.ObjectPools.Abstractions.Lib.Abstractions;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;

namespace IziHardGames.NodeProxies.Graphs
{
    internal sealed class RegistryForAdvancing : IIziNodeAdvancingSearcher
    {
        private readonly Dictionary<Node, NextNode> queue = new Dictionary<Node, NextNode>();

        internal AdvanceResult GetAdvancing(int variant, Node node, IziGraph graph)
        {
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
            else if (node is NodeSmartProxyTcp nodeSmartProxyTcp)
            {

                throw new System.NotImplementedException();
            }
            else
            {
                throw new System.NotImplementedException($"type:{node.GetType().FullName}");
            }
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