namespace IziHardGames.MappedFrameReader
{
    internal class NodeCompare : Node
    {
        public NodeCompare() : base()
        {
            this.typeNode = ENodeType.Compare;
        }
        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override ReadOnlyMemory<byte> GetResultStripped()
        {
            throw new NotImplementedException();
        }
    }
}