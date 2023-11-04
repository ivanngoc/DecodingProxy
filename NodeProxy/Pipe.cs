using System;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.NodeProxy.Nodes;

namespace IziHardGames.NodeProxy.Pipes
{
    internal class Pipe
    {
        public Node? Head { get; set; }
        internal Task? task;
        private Schema schema;

        public Pipe(Schema schema)
        {
            this.schema = schema;
        }

        internal void Start(CancellationToken ct = default)
        {
            if (task != null) throw new ArgumentException("task must be null");
            task = Task.Run(async () =>
            {
                Node nextNode = Head!;
                while (nextNode != null)
                {
                    var node = nextNode;
                    if (node.isConveyorStage)
                    {
                        Task run = node.Start();
                        nextNode = schema.GetNextNode(node);
                    }
                    else if (node.isAsync)
                    {
                        await node.ExecuteAsync(ct).ConfigureAwait(false);
                    }
                    else
                    {
                        node.Execute();
                    }
                }
            });
        }
    }
}
