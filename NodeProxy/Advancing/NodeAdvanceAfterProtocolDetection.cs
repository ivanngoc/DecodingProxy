﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Graphs.Abstractions.Lib;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using IziHardGames.NodeProxies.Graphs;
using IziHardGames.NodeProxies.Nodes;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.NodeProxies.Advancing
{
    internal class NodeAdvanceAfterSocket : IIziNodeAdvancingAdapter
    {
        internal static AdvanceResult Advance(RegistryForAdvancing registry, NodeSocket nodeSocket, IziGraph graph)
        {
            bool isOrigin = nodeSocket is NodeSocketOrigin;

            var indx = graph.indexators[typeof(Indx)].As<Indx>();
            AdvanceResult result = IziPool.GetConcurrent<AdvanceResult>();
            var socket = nodeSocket.Socket;
            NodeConnectionControl control = IziPool.GetConcurrent<NodeConnectionControl>();
            control.SetDynamicFlags(EDynamicStates.AwaitNextNode);
            NextNode nextNodeControl = new NextNode(control, ERelations.Next | ERelations.ControllingObserver | ERelations.RefSource);
            result.Add(nextNodeControl);
            indx[isOrigin ? INDX_ORIGIN_CONN_CONTROL : INDX_AGENT_CONN_CONTROL] = control;

            NodeSocketReader nodeSocketReader = IziPool.GetConcurrent<NodeSocketReader>();
            nodeSocketReader.SetControl(control);
            nodeSocketReader.SetSocket(socket);
            NextNode nextNodeSocketReader = new NextNode(nodeSocketReader, ERelations.Next | ERelations.RefSource);
            result.Add(nextNodeSocketReader);
            indx[isOrigin ? INDX_ORIGIN_SOCKET_READER : INDX_AGENT_SOCKET_READER] = nodeSocketReader;

            if (!isOrigin)
            {
                var agentGate = IziPool.GetConcurrent<NodeGate>();
                indx[INDX_GATE_AGENT] = agentGate;
                registry.QueueNode(nodeSocketReader, agentGate, ERelations.Next | ERelations.FragPeek);
            }
            else
            {
                var nodeSocketClient = indx[INDX_CLIENT_SOCKET] as NodeSocket ?? throw new NullReferenceException();
                var nodeSocketOrigin = indx[INDX_ORIGIN_SOCKET] as NodeSocket ?? throw new NullReferenceException();

                NodeBridgeSocket bridge = new NodeBridgeSocket();
                indx[INDX_SOCKET_BRIDGE] = bridge;

                bridge.SetA(nodeSocketClient);
                bridge.SetB(nodeSocketOrigin);
                result.Add(new NextNode(bridge, ERelations.Next));

                NodeBridgeProxy nodeProxyBridge = IziPool.GetConcurrent<NodeBridgeProxy>();
                result.Add(new NextNode(nodeProxyBridge, ERelations.Next));
            }

            NodeSocketWriter nodeSocketWriter = IziPool.GetConcurrent<NodeSocketWriter>();
            nodeSocketWriter.SetControl(control);
            nodeSocketWriter.SetSocket(socket);
            nodeSocketWriter.SetDynamicFlags(EDynamicStates.AwaitNextNode);
            NextNode nextNodeSocketWriter = new NextNode(nodeSocketWriter, ERelations.Next | ERelations.RefSource);
            result.Add(nextNodeSocketWriter);
            indx[isOrigin ? INDX_ORIGIN_SOCKET_WRITER : INDX_AGENT_SOCKET_WRITER] = nodeSocketWriter;
            return result;
        }
    }

    internal class NodeAdvanceAfterProtocolDetection : IIziNodeAdvancingAdapter
    {

    }
}
