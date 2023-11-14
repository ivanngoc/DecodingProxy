using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeSmartProxyTcp : Node
    {
        public NodeConnectionControl Control => control!;
        private readonly NodeConnectionControl control = new NodeConnectionControl();
        private readonly NodeAwaitDissonnect nodeAwaitDissonnect;
        private readonly NodeDemuxNonBlocking demux;
        public readonly NodeSocketReader reader;
        public readonly NodeSocketWriter writer;
        private readonly List<Task> tasks = new List<Task>();
        private bool isContinuationChoosed;

        public NodeSmartProxyTcp()
        {
            nodeAwaitDissonnect = new NodeAwaitDissonnect(control);
            reader = new NodeSocketReader(control);
            writer = new NodeSocketWriter(control);
            this.demux = new NodeDemuxNonBlocking();
            this.SetNext(control);
            control.SetNext(demux);
            demux.AddOut(reader);
            demux.AddOut(writer);
        }
        public void SetSocket(Socket socket)
        {
            control.SetSocket(socket);
            reader.SetSocket(socket);
            writer.SetSocket(socket);
        }

        public AdaptableSmartProgressingNode ContinuationSetSmart()
        {
            isContinuationChoosed = true;
            var progressor = new AdaptableSmartProgressingNode(this);
            reader.SetNext(progressor);
            NodeMuxNonBlocking nodeMux = new NodeMuxNonBlocking();
            demux.SetNext(nodeMux);
            nodeMux.AddIn(progressor);
            nodeMux.AddIn(writer);
            writer.SetNext(nodeMux);
            progressor.SetNext(nodeMux);
            nodeMux.SetNext(nodeAwaitDissonnect);
            return progressor;
        }

        internal override async Task ExecuteAsync(CancellationToken ct = default)
        {
            Console.WriteLine("Begin ExecuteAsync");
            if (!isContinuationChoosed) throw new InvalidOperationException($"You must specify continuation");
            var node = Next;
            if (tasks.Count > 0) throw new InvalidOperationException($"There is still unfinished tasks");
            // Захват переменной. Когда задача будет запущена, то будет захвачена текущая переменная node
            while (!ct.IsCancellationRequested && node != null)
            {
                Console.WriteLine($"NodeType:{node.GetType().Name}\tflags:{Node.FlagsToInfo(node.flags)}");

                if (node.flags == (ENodeRunFlags.NoTransition | ENodeRunFlags.Sustainable | ENodeRunFlags.Async))
                {
                    var t1 = RunAsync(node, ct);
                    tasks.Add(t1);
                    break;
                }
                else if (node.flags == (ENodeRunFlags.Awaitable))
                {
                    await node.ExecuteAsync(ct).ConfigureAwait(false);
                }
                else if (node.flags == (ENodeRunFlags.Sync))
                {
                    node.Execute();
                }
                else if (node.flags == (ENodeRunFlags.Sync | ENodeRunFlags.Sustainable))
                {
                    var t1 = RunSync(node, ct);
                    tasks.Add(t1);
                }
                else if (node.flags == (ENodeRunFlags.Async | ENodeRunFlags.Sustainable))
                {
                    var t1 = RunAsync(node, ct);
                    tasks.Add(t1);
                }
                else
                {
                    throw new System.NotImplementedException($"Flag as Value decimical:{(int)node.flags}; as enum:{node.flags}");
                }
                node = node.Next;
            }
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }
    }
}
