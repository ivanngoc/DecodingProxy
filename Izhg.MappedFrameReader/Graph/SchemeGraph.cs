namespace IziHardGames.MappedFrameReader
{
    internal class SchemeGraph
    {
        public List<GraphNode> nodes = new List<GraphNode>();
        private int nodeIndex = default;

        public SchemeGraph(NodeBegin nodeBegin)
        {
            Node? next = nodeBegin;

            while (next != null)
            {
                InsertNode(ref next);
            }
        }

        private void InsertNode(ref Node next)
        {
            try
            {
                Console.WriteLine($"Inserted. name:{next.path}.\t\tType:{next.GetType().Name}");
                Node prev = next;
                GraphNode graphNode = new GraphNode();
                graphNode.index = nodeIndex;
                graphNode.value = next;
                nodes.Add(graphNode);
                next = next.Tail;

                if (next is NodeSwitch swBegin)
                {
                    var cases = swBegin.cases;
                    int count = cases.Count;
                    graphNode.ins.Add(prev);
                    for (int i = 0; i < cases.Count; i++)
                    {
                        var item = cases[i];
                        var node = item.node as Node;
                        InsertNode(ref node);
                        graphNode.outs.Add(node);
                    }
                }
                else if (next is NodeSwitchEnd swEnd)
                {
                    var cases = swEnd.cases;
                    var count = cases.Count;
                    graphNode.outs.Add(next);
                    for (int i = 0; i < count; i++)
                    {
                        var item = cases[i];
                        var node = item.node;
                        graphNode.ins.Add(node);
                    }
                }
                else if (next is not NodeBegin)
                {
                    graphNode.outs.Add(next);
                }
                else if (next is not NodeEnd)
                {
                    graphNode.ins.Add(prev);
                }
                else
                {
                    graphNode.ins.Add(prev);
                    graphNode.outs.Add(next);
                }
                nodeIndex++;
            }
            catch (Exception ex)
            {

            }
        }
    }
}