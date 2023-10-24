namespace IziHardGames.MappedFrameReader
{
    /// <summary>
    /// Read With Condition To Stop
    /// </summary>
    internal class NodeReadWithCondition : Node, IAdvancingNode
    {
        private ECondition condition;
        private byte[] bytes;

        public NodeReadWithCondition() : base()
        {
            typeNode = ENodeType.ReadCondition;
        }

        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            Span<byte> seq = bytes.AsSpan();
            var span = mem.Span;
            for (int i = 0; i < span.Length; i++)
            {
                var temp = span.Slice(i);
                if (temp.StartsWith(seq))
                {
                    lengthConsumed = i + seq.Length;
                    if (condition == ECondition.SearchUntilInclusive)
                    {
                        lengthValue = lengthConsumed;
                    }
                    else if (condition == ECondition.SearchUntilExclusive)
                    {
                        lengthValue = i;
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                    SetResultStripped(mem.Slice(0, lengthValue));
                    SetResult(mem.Slice(0, lengthConsumed));
                    return base.ExecuteAsync(mem, readerContext);
                }
            }
            return taskNotFounded;
        }

        internal void SetCondition(ECondition condition, byte[] bytes)
        {
            this.condition = condition;
            this.bytes = bytes;
        }
    }
}