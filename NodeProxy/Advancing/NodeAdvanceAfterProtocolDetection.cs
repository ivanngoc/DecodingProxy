using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Graphs.Abstractions.Lib;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using IziHardGames.NodeProxies.Graphs;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.ObjectPools.Abstractions.Lib.Abstractions;

namespace IziHardGames.NodeProxies.Advancing
{

    internal class NodeAdvanceAfterSocket : IIziNodeAdvancingAdapter
    {
        internal static AdvanceResult Advance(RegistryForAdvancing registry, NodeSocket nodeSocket, IziGraph graph)
        {
            AdvanceResult result = IziPool.GetConcurrent<AdvanceResult>();
            var socket = nodeSocket.Socket;
            NodeConnectionControl control = IziPool.GetConcurrent<NodeConnectionControl>();
            control.SetDynamicFlags(EDynamicStates.AwaitNextNode);
            NextNode nextNodeControl = new NextNode(control, ERelations.Next | ERelations.ControllingObserver | ERelations.RefSource);
            result.Add(nextNodeControl);

            NodeSocketReader nodeSocketReader = IziPool.GetConcurrent<NodeSocketReader>();
            nodeSocketReader.SetControl(control);
            nodeSocketReader.SetSocket(socket);
            NextNode nextNodeSocketReader = new NextNode(nodeSocketReader, ERelations.Next | ERelations.RefSource);
            result.Add(nextNodeSocketReader);
            registry.QueueNode(nodeSocketReader, new NodeGate(), ERelations.Next | ERelations.FragPeek);

            NodeSocketWriter nodeSocketWriter = IziPool.GetConcurrent<NodeSocketWriter>();
            nodeSocketWriter.SetControl(control);
            nodeSocketWriter.SetSocket(socket);
            nodeSocketWriter.SetDynamicFlags(EDynamicStates.AwaitNextNode);
            NextNode nextNodeSocketWriter = new NextNode(nodeSocketWriter, ERelations.Next | ERelations.RefSource);
            result.Add(nextNodeSocketWriter);
            return result;
        }
    }

    internal class NodeAdvanceAfterProtocolDetection : IIziNodeAdvancingAdapter
    {

    }
}
