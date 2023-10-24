namespace IziHardGames.MappedFrameReader
{
    internal class NodesQueue
    {
        public readonly Queue<Node> nodes = new Queue<Node>();

        public void Push(Node node)
        {
            nodes.Enqueue(node);
        }

        public void Reset()
        {
            nodes.Clear();
        }
    }

    public class TableOfResults
    {
        internal Dictionary<string, Result> results = new Dictionary<string, Result>();
        internal void AddDynamic(int vectorCounter, string path, NodeResult nodeResult)
        {
            string finalPath = $"{path}[{vectorCounter}]";
            nodeResult.SetPath(finalPath);
            Result result = new Result()
            {
                path = finalPath,
                index = vectorCounter,
                nodeResult = nodeResult,
            };
            results.Add(finalPath, result);
        }

        internal void AddResult(NodeResult nodeResult)
        {
            string path = nodeResult.path;
            Result result = new Result()
            {
                path = path,
                index = 0,
                nodeResult = nodeResult,
            };
            results.Add(path, result);
        }

        internal Result GetResult(string path)
        {
            return results[path];
        }

        internal class Result
        {
            public int index;
            public string path;
            public NodeResult nodeResult;
        }
    }

    internal class TableOfFrames
    {
        private readonly Dictionary<string, DefinedType> types = new Dictionary<string, DefinedType>();
    }
}