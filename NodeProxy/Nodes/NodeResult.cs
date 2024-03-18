using System;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class NodeResult
    {
        private string result;
        public bool Is(string compare)
        {
            return string.Compare(result, compare, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
