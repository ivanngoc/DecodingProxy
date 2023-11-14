using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes.SOCKS5
{
    internal class NodeTcpClient : BidirectionalNode
    {
        private TcpClient? tcpClient;
        private NodeConnectionControl? control;
        public NodeConnectionControl Control => control ?? throw new NullReferenceException();

        private IPAddress? iPAddress;
        private ushort port;

        public NodeTcpClient() : base()
        {

        }
        public NodeTcpClient(TcpClient tcpClient) : base()
        {
            if (!tcpClient.Connected) throw new ArgumentException("client must be connected");
            this.tcpClient = tcpClient;
            this.control = new NodeConnectionControl();
            this.SetNext(control);
            control.SetSocket(tcpClient.Client);
        }

        public void SetConnectionAddress(IPAddress iPAddress, ushort port)
        {
            this.iPAddress = iPAddress;
            this.port = port;
        }
        public void SetTcpClient(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
        }
        public async Task ConnectAsync()
        {
            await tcpClient!.ConnectAsync(iPAddress!, port).ConfigureAwait(false);
        }

        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }
        internal override async Task ExecuteAsync(CancellationToken ct)
        {
            var t1 = RunWriteLoop(ct);
            var t2 = RunReadLoop(ct);
            var t3 = RunAsync(control!, ct);
            await control!.AwaitCompletion(ct).ConfigureAwait(false);
        }

        private Task RunReadLoop(CancellationToken ct)
        {
            return Task.Run(async () =>
                 {
                     var tcpClient = this.tcpClient!;
                     while (!ct.IsCancellationRequested)
                     {
                         var stream = tcpClient.GetStream();
                         DataFragment? frag = null;
                         if (tcpClient.Available > 0)
                         {
                             frag = DataFragment.Get(tcpClient.Available);
                         }
                         else
                         {
                             frag = DataFragment.Get((1 << 10) * 512);
                         }
                         REPEAT:
                         int readed = await stream.ReadAsync(frag!.buffer, ct).ConfigureAwait(false);
                         if (readed > 0)
                         {
                             frag.SetLength(readed);
                             lock (fragmentsToTakeOut)
                             {
                                 fragmentsToTakeOut.Enqueue(frag);
                             }
                             control!.ReportRead(readed);
                         }
                         else
                         {
                             await Task.Delay(200).ConfigureAwait(false);
                             goto REPEAT;
                         }
                     }
                 });
        }
        private Task RunWriteLoop(CancellationToken ct)
        {
            return Task.Run(async () =>
            {
                var tcpClient = this.tcpClient!;
                var stream = tcpClient.GetStream();
                while (!ct.IsCancellationRequested || fragmentsToInsertIn.Count > 0)
                {
                    var frag = await TakeFrameFromIn(ct).ConfigureAwait(false);
                    await stream.WriteAsync(frag.ReadOnly).ConfigureAwait(false);
                    control!.ReportWrite(frag.ReadOnly.Length);
                }
            });
        }
    }
}
