using System.Threading.Tasks;
using System;
using System.Net.Sockets;
using IziHardGames.NodeProxies.Nodes;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Graphs.Abstractions.Lib;
using System.Threading;
using IziHardGames.Graphs.Abstractions.Lib.ValueTypes;
using System.Collections.Generic;
using System.Linq;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using IziHardGames.Pools.Abstractions.NetStd21;

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

        public Task RunAsyncV2(Socket socket, CancellationToken ct)
        {
            var indx = graph.indexators[typeof(Indx)].As<Indx>();
            NodeSocket node = new NodeSocket();
            node.SetSocket(socket);
            IziNode iziNode = graph!.GetNewNode();
            Associate(iziNode, node);
            indx[INDX_CLIENT_SOCKET] = node;
            return Iterate(iziNode, ct);
        }
        public Task RunAsyncV1(Socket socket, CancellationToken ct)
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
            node.SetGraph(graph!);
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

                    if (!node.Validate()) throw new InvalidOperationException();

                    var relations = nodeRelations![iziNode];

                    if (flags == (ENodeRunFlags.NoAdvancing | ENodeRunFlags.Sustainable | ENodeRunFlags.Async))
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
                        var t1 = Node.RunSync(node, ct);
                    }
                    else if (flags == (ENodeRunFlags.Async | ENodeRunFlags.Sustainable))
                    {
                        var t1 = Node.RunAsync(node, ct);
                    }
                    else
                    {
                        throw new System.NotImplementedException($"Flag as Value decimical:{(int)node.flags}; as enum:{node.flags}");
                    }

                    if (!flags.HasFlag(ENodeRunFlags.NoAdvancing) && !node.DynamicStates.HasFlag(EDynamicStates.AwaitNextNode))
                    {
                        var adv = registryForAdvancing.GetAdvancing(default, node, graph);
                        if (adv.NextNodes.Count() > 0)
                        {
                            foreach (var next in adv.NextNodes)
                            {
                                var nextNode = next.node;
                                var relation = next.relation;
                                var nextIziNode = graph!.GetNewNode();
                                nextNode.id = nextIziNode.id;
                                Associate(nextIziNode, nextNode);
                                nodeRelations.CreateRelationship(iziNode, nextIziNode, (int)relation);

                                if (next.relation.HasFlag(ERelations.FragPeek))
                                {
                                    (nextNode as IFragsPeeker)!.SetSourceForShowing(node as IFragsShowing ?? throw new NullReferenceException());
                                }
                                await Iterate(nextIziNode, ct).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            throw new System.NotImplementedException("No Advancing founded Without NoAdvancing flag raised");
                        }
                        IziPool.ReturnConcurrent(adv);
                    }
                }
            });
        }

        public async Task CascadeUpdateBackward(Node start)
        {
            throw new System.NotImplementedException();
        }

        internal static ProxyNodeAdvancer GetNew()
        {
            return new ProxyNodeAdvancer();
        }
    }
    internal sealed class AdvanceResult : IDisposable
    {
        private readonly List<NextNode> nextNodes = new List<NextNode>();
        public IEnumerable<NextNode> NextNodes => nextNodes;
        public int variant;

        public void Dispose()
        {
            nextNodes.Clear();
        }

        internal void Add(NextNode nextNodeControl)
        {
            nextNodes.Add(nextNodeControl);
        }
        public AdvanceResult AsVariant(int variant)
        {
            this.variant = variant;
            return this;
        }

    }

    internal struct NextNode
    {
        public Node node;
        public ERelations relation;

        public NextNode(Node node, ERelations relation)
        {
            this.node = node;
            this.relation = relation;
        }
    }
}