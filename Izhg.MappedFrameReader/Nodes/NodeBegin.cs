namespace IziHardGames.MappedFrameReader
{
    internal class NodeBegin : Node
    {
        public NodeBegin() : base()
        {
            this.path = nameof(NodeBegin);
            this.typeNode = ENodeType.Begin;
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
    }
}