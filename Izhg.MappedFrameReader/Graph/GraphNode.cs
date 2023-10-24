namespace IziHardGames.MappedFrameReader
{
    internal class GraphNode
    {
        public int index;
        public Node? value;
        public readonly List<Node> ins = new List<Node>();
        public readonly List<Node> outs = new List<Node>();
    }
}