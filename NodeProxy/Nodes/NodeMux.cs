using System.Collections.Generic;

namespace IziHardGames.NodeProxies.Nodes
{
    /// <summary>
    /// Multiple Ins
    /// Single Out
    /// </summary>
    internal class NodeMux : Node
    {
        protected readonly List<Node> ins = new List<Node>();
        public void AddIn(Node inNode)
        {
            ins.Add(inNode);
        }
    }
}
