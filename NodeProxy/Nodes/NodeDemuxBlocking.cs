using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeDemux : Node
    {
        public readonly List<Node> outes = new List<Node>();
        internal void AddOut(Node node)
        {
#if DEBUG
            if (outes.Contains(node)) throw new ArgumentException("Node already added to outes!");
#endif
            outes.Add(node);
        }
    }
    /// <summary>
    /// Single In
    /// Muiltiple Outs
    /// </summary>
    internal class NodeDemuxBlocking : NodeDemux
    {
        private readonly List<Task<List<Task>>> tasks = new List<Task<List<Task>>>();

        public NodeDemuxBlocking() : base()
        {

        }

        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Awaitable;
        }

        internal override async Task ExecuteAsync(CancellationToken ct)
        {
            Console.WriteLine($"{GetType().FullName}.ExecuteAsync()");
            if (tasks.Count > 0) throw new InvalidOperationException($"There is still unfinished tasks");

            foreach (var node in outes)
            {
                var t1 = RunItterate(node, Next!, ct);
                tasks.Add(t1);
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Single In
    /// Muiltiple Outs
    /// </summary>
    internal class NodeDemuxNonBlocking : NodeDemux
    {
        private readonly List<Task<List<Task>>> tasks = new List<Task<List<Task>>>();
        public NodeDemuxNonBlocking() : base()
        {

        }
        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Sustainable | ENodeRunFlags.Sync;
        }
        internal override void Execute()
        {
            throw new System.NotImplementedException();
        }
        internal override Task ExecuteAsync(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
        internal override void ExecuteParallel(CancellationToken ct = default)
        {
            Console.WriteLine($"{GetType().FullName}.ExecuteParallel()");

            if (tasks.Count > 0) throw new InvalidOperationException($"There is still unfinished tasks");

            foreach (var node in outes)
            {
                var t1 = RunItterate(node, Next!, ct);
                tasks.Add(t1);
            }
        }
    }
}
