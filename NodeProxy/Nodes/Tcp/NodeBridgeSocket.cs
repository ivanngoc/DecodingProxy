using System;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeBridgeSocket : Node, IBridge
    {
        private NodeSocket? a;
        private NodeSocket? b;

        internal void SetA(NodeSocket a)
        {
            this.a = a;
        }
        internal void SetB(NodeSocket b)
        {
            this.b = b;
        }

        internal override void Execute()
        {
            base.Execute();
        }

        public override ENodeRunFlags GetRunFlags()
        {
            return ENodeRunFlags.Sync;
        }
    }
}
