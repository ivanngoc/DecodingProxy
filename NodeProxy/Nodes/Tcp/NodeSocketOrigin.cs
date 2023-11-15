using System.Threading;
using System.Threading.Tasks;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<string, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using IziHardGames.Libs.Async;
using IziHardGames.NodeProxies.Nodes.SOCKS5;
using System;
using System.Net;
using System.Net.Sockets;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeSignaler : Node
    {
        public readonly AsyncSignaler signaler = new AsyncSignaler();
        internal void SetSync(Node node, Node nodeSocketOrigin)
        {
            this.a = node;
            this.b = nodeSocketOrigin;
        }
    }

    internal class NodeSocketOrigin : NodeSocket
    {
        internal override Task ExecuteAsync(CancellationToken ct)
        {
            var index = graph!.indexators[typeof(Indx)].As<Indx>();
            index[INDX_ORIGIN_SOCKET] = this;
            var nodeSocks5 = index[INDX_SOCKS_GREET];
            return base.ExecuteAsync(ct);
        }

        public async Task ConnectAsync(IPAddress destinationIpAddress, ushort destinationPort)
        {
            socket = new Socket(destinationIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(destinationIpAddress, destinationPort).ConfigureAwait(false);
        }
        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Async | ENodeRunFlags.Sustainable;
        }
        public override ETraits GetTraits()
        {
            return ETraits.Error;
        }
    }
}
