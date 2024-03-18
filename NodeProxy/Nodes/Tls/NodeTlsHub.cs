using Indx = IziHardGames.Graphs.Abstractions.Lib.ValueTypes.Indexator<int, IziHardGames.NodeProxies.Nodes.Node>;
using static IziHardGames.NodeProxies.Advancing.ConstantsForNodeProxy;
using System;

namespace IziHardGames.NodeProxies.Nodes.Tls
{
    internal class NodeTlsHub : NodeTls
    {
        public NodeTlsFramesReader? reader;
        public NodeTlsFramesWriter? writer;
        public NodeTlsAuth? auth;

        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Sync;
        }

        internal override void Execute()
        {

        }

        internal void SetAuth(NodeTlsAuth auth)
        {
            this.auth = auth;
        }

        internal void SetReader(NodeTlsFramesReader reader)
        {
            this.reader = reader;
        }

        internal void SetWriter(NodeTlsFramesWriter writer)
        {
            this.writer = writer;
        }
    }
}
