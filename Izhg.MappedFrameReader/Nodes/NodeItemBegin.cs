namespace IziHardGames.MappedFrameReader
{
    internal class NodeItemEnd : Node
    {
        public NodeItemEnd()
        {

        }
        internal override void Execute(ReaderContext readerContext)
        {

        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            return taskSucceed;
        }
    }

    internal class NodeItemBegin : Node
    {
        private DefinedType definedType;
        private NodeItemEnd end;

        public NodeItemBegin() : base()
        {
            this.typeNode = ENodeType.ItemBegin;
        }
        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            return Task.CompletedTask;
        }

        internal override ReadOnlyMemory<byte> GetResultStripped()
        {
            return default;
        }

        internal void SetType(DefinedType definedType)
        {
            this.definedType = definedType;
        }
        internal void SetEnd(NodeItemEnd end)
        {
            this.end = end;
        }
    }
}