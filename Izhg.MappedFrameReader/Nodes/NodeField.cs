namespace IziHardGames.MappedFrameReader
{
    internal class NodeField : Node
    {
        private DefinedType definedType;

        public NodeField() : base()
        {
            this.typeNode = ENodeType.BeginField;
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


        internal void SetType(DefinedType definedType)
        {
            this.definedType = definedType;
        }
    }
}