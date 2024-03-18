using System.Threading;
using System.Threading.Tasks;
using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using System;
using IziHardGames.NodeProxies.Nodes.Tls;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeBridgeProxy : Node, IBridge
    {
        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Awaitable;
        }
        internal override Task ExecuteAsync(CancellationToken ct)
        {
            var indx = graph!.indexators[typeof(Indx)].As<Indx>();

            var hubOrigin = indx[INDX_TLS_SERVER_HUB] as NodeTlsHub;
            var hubClient = indx[INDX_TLS_CLIENT_HUB] as NodeTlsHub;

            var readerOrigin = indx[INDX_TLS_SERVER_READER];
            var writerOrigin = indx[INDX_TLS_SERVER_WRITER];

            var readerAgent = indx[INDX_TLS_CLIENT_READER];
            var writerAgent = indx[INDX_TLS_CLIENT_WRITER];

            return Task.CompletedTask;
        }
    }
}
