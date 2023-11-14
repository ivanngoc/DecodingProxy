using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Graphs.Abstractions.Lib;

namespace IziHardGames.NodeProxies.Nodes
{

    internal class NodeStart : Node
    {

    }
    internal class NodeEnd : Node
    {

    }

    /// <summary>
    /// Control Tcp connection
    /// </summary>
    internal class NodeTcpAcceptStart : NodeDemuxBlocking
    {
        private Socket? socket;
        private NodeRead read;
        private NodeWrite write;

        public NodeTcpAcceptStart(NodeRead read, NodeWrite write, NodeTcpAcceptEnd end) : base()
        {
            this.read = read;
            this.write = write;
            outes.Add(read);
            outes.Add(write);
            Next = end;
        }
        internal void Bind(Socket socket)
        {
            this.socket = socket;
        }
    }

    internal class NodeTcpAcceptEnd : NodeMux
    {

    }

    internal abstract class NodeWrite : Node
    {

    }
    internal abstract class NodeRead : Node
    {

    }

    internal class NodeTcpWrite : NodeWrite
    {

    }

    internal class NodeTcpRead : NodeRead
    {

    }
    internal abstract class Node : IIziNode, IDisposable
    {
        public virtual Node? Next { get; set; }
        public virtual Node? Previous { get; set; }
        private bool isDisposed = true;
        public bool isAsync;
        /// <summary>
        /// Долго живущая нода. Как IHostedService в ASP.NEt. 
        /// Позволяет к примеру создавать <see cref="NodeData"/> которые  будут стакаться. 
        /// Стадия конвейера
        /// </summary>
        public bool isConveyorStage;
        public NodeIterator iterator;

        public readonly ETraits traits;
        public readonly ENodeRunFlags flags;

        protected Node()
        {
            flags = GetRunFlags();
        }

        internal void SetNext(Node next)
        {
            if (this.flags.HasFlag(ENodeRunFlags.NoTransition)) throw new ArgumentException($"{GetType().FullName}. this Node has flag no transition thats why there can't be any next node");
            this.Next = next;
            next.SetPrevious(this);
        }
        internal void SetPrevious(Node node)
        {
            this.Previous = node;
        }
        internal virtual void Initilize()
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed before use!");
            isDisposed = false;
        }
        internal virtual void Execute()
        {
            throw new System.NotImplementedException();
        }
        internal virtual void ExecuteParallel(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
        internal virtual Task ExecuteAsync(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
        internal virtual Task AwaitCompletion(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
        public virtual NodeResult GetResult() { throw new System.NotImplementedException(); }

        public virtual void RunDependecies()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("Object is already disposed. you must explicitly call Initilize() before each usage");
            isDisposed = true;
        }
        internal virtual Task Start()
        {
            throw new NotImplementedException();
        }
        public virtual ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.ErrorNotOverrided;
        }
        public virtual ETraits GetTraits() => ETraits.ErrorNotOverrided;

        public static async Task<List<Task>> Itterate(Node start, Node end, CancellationToken ct = default)
        {
            Console.WriteLine($"Node.Itterate; from:{start.GetType().Name} to:{end.GetType().Name}");

            if (end == null) throw new NullReferenceException($"End is mull. End must be specified");
            Node node = start;
            List<Task> tasks = new List<Task>();

            while (!ct.IsCancellationRequested && node != end)
            {
                var flags = start.flags;
                Console.WriteLine($"Node.Itterate(); Current:{node.GetType().Name} flags:{Node.FlagsToInfo(flags)}");
                if (flags.HasFlag(ENodeRunFlags.NoTransition)) throw new FormatException($"This method not allowed no transitions");

                if (flags == (ENodeRunFlags.Awaitable))
                {
                    await node.ExecuteAsync(ct).ConfigureAwait(false);
                }
                else if (flags == (ENodeRunFlags.Sync))
                {
                    node.Execute();
                }
                else if (flags == (ENodeRunFlags.Sustainable | ENodeRunFlags.Async))
                {
                    var t1 = RunAsync(node, ct);
                    tasks.Add(t1);
                }
                else if (flags == (ENodeRunFlags.Sustainable | ENodeRunFlags.Sync))
                {
                    var t1 = RunSync(node, ct);
                    tasks.Add(t1);
                }
                else
                {
                    throw new System.NotImplementedException($"Flag as Value decimical:{(int)start.flags}; as enum:{start.flags}");
                }
                node = node.Next!;
                if (node == null) throw new NullReferenceException($"next node is null");
            }
            return tasks;
        }

        protected static string FlagsToInfo(ENodeRunFlags flags)
        {
            string result = string.Empty;
            if (flags == ENodeRunFlags.NoTransition) return nameof(ENodeRunFlags.NoTransition);
            if (flags.HasFlag(ENodeRunFlags.Sync)) result += nameof(ENodeRunFlags.Sync) + ';';
            if (flags.HasFlag(ENodeRunFlags.Async)) result += nameof(ENodeRunFlags.Async) + ';';
            if (flags.HasFlag(ENodeRunFlags.Sustainable)) result += nameof(ENodeRunFlags.Sustainable) + ';';
            if (flags.HasFlag(ENodeRunFlags.Awaitable)) result += nameof(ENodeRunFlags.Awaitable) + ';';
            if (flags.HasFlag(ENodeRunFlags.NoTransition)) result += nameof(ENodeRunFlags.NoTransition) + ';';
            return result;
        }

        protected static Task RunSync(Node node, CancellationToken ct = default)
        {
            return Task.Run(() => node.ExecuteParallel(ct));
        }
        protected static Task RunAsync(Node node, CancellationToken ct = default)
        {
            return Task.Run(async () => await node.ExecuteAsync(ct).ConfigureAwait(false));
        }
        protected static Task<List<Task>> RunItterate(Node node, Node to, CancellationToken ct = default)
        {
            return Task.Run(() => Itterate(node, to, ct));
        }
    }

    internal class NodeProtocolDetection : Node
    {

    }

    internal abstract class NodeBuffer : Node
    {

    }
    internal class NodeMuxNonBlocking : NodeMux
    {
        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Sync | ENodeRunFlags.Sustainable;
        }
        internal override void ExecuteParallel(CancellationToken ct = default)
        {

        }
    }

    /// <summary>
    /// Multiple Ins
    /// Single Out
    /// </summary>
    internal class NodeMux : Node
    {
        protected readonly List<Node> ins = new List<Node>();
        public void AddIn(Node inNode)
        {
            ins.Add(inNode);
        }
    }
    internal class NodeEnumerator : Node
    {
        private IEnumerator<Node> enumerator;

        public NodeEnumerator(IEnumerator<Node> outes)
        {
            this.enumerator = outes;
        }
        public override Node? Next { set => throw new System.NotSupportedException(); get => MoveNext(); }
        private Node? MoveNext()
        {
            if (enumerator.MoveNext())
            {
                return enumerator.Current;
            }
            throw new System.NotImplementedException();
        }
    }
    internal class NodeData
    {

    }
    internal class NodeResult
    {
        private string result;
        public bool Is(string compare)
        {
            return string.Compare(result, compare, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
    internal class NodeReadHttp11 : Node
    {

    }
    internal class NodeHttpConnect : Node
    {

    }
}
