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
        private Node? startNode;
        private readonly RegistryForAdvancing registryForAdvancing = new RegistryForAdvancing();
        private IziNodeRelations? nodeRelations;

        public void SetRelations<T>(T relations) where T : IIziNodesRelations
        {
            this.nodeRelations = relations as IziNodeRelations ?? throw new ArgumentException();
        }

        public Task RunAsync(Socket socket, CancellationToken ct)
        {
            NodeSmartProxyTcp node = new NodeSmartProxyTcp();
            node.SetSocket(socket);
            node.ContinuationSetSmart();
            return Iterate(node, ct);
        }

        internal Task Iterate(Node start, CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var node = start;
                while (!ct.IsCancellationRequested)
                {
                    var traits = node.GetTraits();
                    var flags = node.GetRunFlags();

                    nodeRelations.GetRelations(node);

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
                        nodeRelations.SetRelation(node, next, (int)ERelations.Next);
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