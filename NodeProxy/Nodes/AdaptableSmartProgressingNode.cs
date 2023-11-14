using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.NodeProxies.Nodes.SOCKS5;
using IziHardGames.NodeProxies.Nodes.Tls;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class AdaptableSmartProgressingNode : NodeRead
    {
        private readonly NodeSmartProxyTcp nodeSmartProxyTcp;
        private readonly NodeSocketReader reader;
        private readonly NodeSocketWriter writer;

        public AdaptableSmartProgressingNode(NodeSmartProxyTcp nodeSmartProxyTcp)
        {
            this.nodeSmartProxyTcp = nodeSmartProxyTcp;
            this.reader = nodeSmartProxyTcp.reader;
            this.writer = nodeSmartProxyTcp.writer;
        }
        internal override async Task ExecuteAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                var frag = await reader.TakeFragAsync(ct).ConfigureAwait(false);
                var protocol = NodeGate.DetectProtocol(frag);

                Console.WriteLine($"Detected protocol: {protocol}");
                Console.WriteLine(ParseByte.ToHexStringFormated(frag.buffer));
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(Encoding.UTF8.GetString(frag.buffer.Span));

                switch (protocol)
                {
                    case EGateProtocol.TLS:
                        {
                            /// <see cref="NodeTlsHandshakeFromClient"/>   
                            throw new System.NotImplementedException();
                        }
                    case EGateProtocol.SOCKS5:
                        {
                            NodeSocks5AsServer socks5 = new NodeSocks5AsServer();
                            this.SetNext(socks5);
                            socks5.InsertFrag(frag);
                            socks5.SetSources(reader, writer);
                            socks5.SetCallback(CheckOverride);
                            socks5.SetNextNodeRequester(DetermineNextNode);
                            var t1 = RunAsync(socks5);
                            await t1.ConfigureAwait(false);
                            break;
                        }
                    default: throw new NotImplementedException(protocol.ToString());
                }
            }
        }

        private async Task<BidirectionalNode> DetermineNextNode(Node node, CancellationToken ct)
        {
            if (node is NodeSocks5AsServer socks5)
            {
                var ip = socks5.DestinationIpAddressObtained;
                var port = socks5.DestinationPortObtained;
                var result = new NodeTcpClient();
                result.SetTcpClient(new TcpClient());
                result.SetConnectionAddress(ip, port);
                await result.ConnectAsync().ConfigureAwait(false);
                SessionControl sessionControl = new SessionControl();
                sessionControl.SetControls(nodeSmartProxyTcp.Control, result.Control);
                socks5.SetSession(sessionControl);
                socks5.SetAwaitingCompletionTask(sessionControl.SessionCompleted(ct));
                return result;
            }
            throw new System.NotImplementedException();
        }

        private Task CheckOverride(Node node)
        {
            return Task.CompletedTask;
        }

        internal override void Execute()
        {
            throw new System.NotImplementedException();
        }
        internal override void ExecuteParallel(CancellationToken ct)
        {
            throw new System.NotImplementedException();
        }
        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Sustainable | ENodeRunFlags.Async;
        }

        public override ETraits GetTraits()
        {
            return ETraits.Advancing;
        }
    }
}
