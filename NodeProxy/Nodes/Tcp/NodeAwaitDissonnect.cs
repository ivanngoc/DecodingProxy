using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeAwaitDissonnect : Node
    {
        private readonly NodeConnectionControl control;
        public NodeAwaitDissonnect(NodeConnectionControl control)
        {
            this.control = control;
        }
        internal override Task ExecuteAsync(CancellationToken ct = default)
        {
            return control.AwaitDisconnect(ct);
        }
        public override ENodeRunFlags GetFlags()
        {
            return ENodeRunFlags.Awaitable;
        }
    }
}
