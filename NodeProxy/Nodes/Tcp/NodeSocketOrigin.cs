using System.Threading;
using System.Threading.Tasks;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
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
        private Node a;
        public Node b;

        internal void SetSync(Node node, Node nodeSocketOrigin)
        {
            this.a = node;
            this.b = nodeSocketOrigin;
        }
    }

    internal class NodeSocketOrigin : NodeSocket
    {
        internal override void Execute()
        {
            var index = graph!.indexators[typeof(Indx)].As<Indx>();
            
            var nodeSocks5 = index[INDX_SOCKS_GREET];
        }

        public async Task ConnectAsync(IPAddress destinationIpAddress, ushort destinationPort)
        {
            socket = new Socket(destinationIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(destinationIpAddress, destinationPort).ConfigureAwait(false);
        }
        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Sync;
        }
        public override ETraits GetTraits()
        {
            return ETraits.Error;
        }
    }
}
