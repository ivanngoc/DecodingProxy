using IziHardGames.Graphs.Abstractions.Lib;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using IziHardGames.NodeProxies.Graphs;
using IziHardGames.NodeProxies.Nodes;

namespace IziHardGames.NodeProxies.Advancing
{
    internal class NodeAdvanceAfterNodeGate : IIziNodeAdvancingAdapter
    {
        internal static AdvanceResult Advance(RegistryForAdvancing registry, NodeGate nodeGate, IziGraph graph)
        {
            if (nodeGate.Protocol == EGateProtocol.SOCKS5)
            {
                IFragTakable source = graph.navigator.FindDescendant<Node>(nodeGate.id, (x) => x is IFragTakable);
            }
        }
    }
}
