namespace IziHardGames.MappedFrameReader
{
    internal class NodeSchemeEnd : Node
    {
        public NodeSchemeEnd()
        {
            this.typeNode = ENodeType.SchemeEnd;

        }
        internal override void Execute(ReaderContext readerContext)
        {

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

    internal class NodeSchemeBegin : Node
    {
        public NodeSchemeBegin() : base()
        {
            this.typeNode = ENodeType.SchemeBegin;
        }

        internal override void Execute(ReaderContext readerContext)
        {

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