namespace IziHardGames.MappedFrameReader
{
    internal class NodeEnd : Node
    {
        public NodeEnd() : base()
        {
            this.path = nameof(NodeEnd);
            this.typeNode = ENodeType.End;
        }
        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            return taskSucceed;
        }

        internal override ReadOnlyMemory<byte> GetResultStripped()
        {
            return default;
        }
    }
}