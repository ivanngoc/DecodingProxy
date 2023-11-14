using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Graphs.Abstractions.Lib;
using System.Threading;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using System.Collections.Generic;

namespace IziHardGames.NodeProxies.Graphs
{
    internal sealed class ProxyNodeAdvancer : INodeAdvancer
    {
        public readonly RegistryForAdvancing registryForAdvancing = new RegistryForAdvancing();
        public readonly StdStore<Node>? store = new StdStore<Node>();

        private IziNodeRelations? nodeRelations;
        private IziGraph? graph;
        private Node? startNode;

        public void FromGraph<T>(T graph) where T : IIziGraph
        {
            var iziGraph = this.graph = graph as IziGraph;
            iziGraph!.associations[typeof(Node)] = store;
        }
        public void SetRelations<T>(T relations) where T : IIziNodesRelations
        {
            this.nodeRelations = relations as IziNodeRelations ?? throw new ArgumentException();
        }

        public Task RunAsync(Socket socket, CancellationToken ct)
        {
            NodeSmartProxyTcp node = new NodeSmartProxyTcp();
            node.SetSocket(socket);
            node.ContinuationSetSmart();
            IziNode iziNode = graph!.GetNewNode();
            Associate(iziNode, node);
            return Iterate(iziNode, ct);
        }

        private void Associate(IziNode iziNode, Node node)
        {
            store!.Associate(iziNode, node);
            node.id = iziNode.id;
        }

        internal Task Iterate(IziNode start, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var iziNode = start;
                var node = store![iziNode];

                while (!ct.IsCancellationRequested)
                {
                    var traits = node.GetTraits();
                    var flags = node.GetRunFlags();

                    var relations = nodeRelations![iziNode];

                    if (flags == (ENodeRunFlags.NoTransition | ENodeRunFlags.Sustainable | ENodeRunFlags.Async))
                    {
                        break;
                    }
                    else if (flags == (ENodeRunFlags.Awaitable))
                    {
                        await node.ExecuteAsync(ct).ConfigureAwait(false);
                    }
                    else if (flags == (ENodeRunFlags.Sync))
                    {
                        node.Execute();
                    }
                    else if (flags == (ENodeRunFlags.Sync | ENodeRunFlags.Sustainable))
                    {
                        var t1 = RunSync(node, ct);
                    }
                    else if (flags == (ENodeRunFlags.Async | ENodeRunFlags.Sustainable))
                    {
                        var t1 = RunAsync(node, ct);
                    }
                    else
                    {
                        throw new System.NotImplementedException($"Flag as Value decimical:{(int)node.flags}; as enum:{node.flags}");
                    }

                    var adv = registryForAdvancing.GetAdvancing(node);

                    foreach (var next in adv.NextNodes)
                    {
                        var nextIziNode = graph!.GetNewNode();
                        Associate(nextIziNode, next);
                        nodeRelations.CreateRelationship(iziNode, nextIziNode, (int)ERelations.Next);
                        await Iterate(start, ct).ConfigureAwait(false);
                    }
                }
            });
        }
        internal static ProxyNodeAdvancer GetNew()
        {
            return new ProxyNodeAdvancer();
        }
    }

    internal sealed class RegistryForAdvancing : IIziNodeAdvancingSearcher
    {
        internal Advancing GetAdvancing(Node node)
        {
            throw new NotImplementedException();
        }
    }
    internal sealed class Advancing
    {
        private readonly List<Node> nextNodes = new List<Node>();
        public IEnumerable<Node> NextNodes => nextNodes;
    }
}