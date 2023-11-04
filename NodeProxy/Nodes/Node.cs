using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxy.Nodes
{
    /// <summary>
    /// Control Tcp connection
    /// </summary>
    internal class NodeTcpAccept : Node
    {
        private Socket? socket;
        public NodeTcpAccept() : base()
        {
            var demux = new NodeDemux();
            this.Next = demux;
            demux.SetPrevious(this);
            demux.AddOut(new NodeTcpRead());
            demux.AddOut(new NodeTcpWrite());
        }
        internal void Bind(Socket socket)
        {
            this.socket = socket;
        }
    }

    internal class NodeWrite : Node
    {

    }
    internal class NodeRead : Node
    {

    }

    internal class NodeTcpWrite : NodeWrite
    {

    }

    internal class NodeTcpRead : NodeRead
    {

    }

    /// <summary>
    /// анализирует первый пакет и выявляет тип протокола
    /// </summary>
    internal class NodeGate : Node
    {

    }
    internal abstract class Node : IDisposable
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

        internal void SetPrevious(Node node)
        {
            this.Previous = node;
        }
        internal virtual void Initilize()
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed before use!");
            isDisposed = false;
        }
        internal virtual void Execute() { }
        internal virtual Task ExecuteAsync(CancellationToken ct) { return Task.CompletedTask; }
        public virtual NodeResult GetResult() { throw new System.NotImplementedException(); }

        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("Object is already disposed. you must explicitly call Initilize() before each usage");
            isDisposed = true;
        }
        internal virtual Task Start()
        {
            throw new NotImplementedException();
        }
    }

    internal class NodeProtocolDetection : Node
    {

    }

    internal abstract class NodeBuffer : Node
    {

    }

    /// <summary>
    /// Single In
    /// Muiltiple Outs
    /// </summary>
    internal class NodeDemux : Node
    {
        public readonly List<Node> outes = new List<Node>();

        public NodeDemux() : base()
        {
            this.Next = new NodeEnumerator(outes.GetEnumerator());
        }

        internal void AddOut(Node node)
        {
#if DEBUG
            if (outes.Contains(node)) throw new ArgumentException("Node already added to outes!");
#endif
            outes.Add(node);
        }
    }
    /// <summary>
    /// Multiple Ins
    /// Single Out
    /// </summary>
    internal class NodeMux : Node
    {

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
