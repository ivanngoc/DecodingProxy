using System.Threading;

namespace IziHardGames.NodeProxies.Nodes
{
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
}
