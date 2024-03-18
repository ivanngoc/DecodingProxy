using System.Collections.Generic;

namespace IziHardGames.NodeProxies.Nodes
{
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
}
