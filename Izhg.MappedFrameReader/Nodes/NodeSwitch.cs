namespace IziHardGames.MappedFrameReader
{
    /// <summary>
    /// Multiplexor
    /// </summary>
    internal class NodeSwitchEnd : Node
    {
        public readonly List<CaseItemEnd> cases = new List<CaseItemEnd>();
        public NodeSwitchEnd() : base()
        {
            this.typeNode = ENodeType.SwitchEnd;
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
            throw new NotImplementedException();
        }

        internal void AddEnd(NodeSwitchItemEnd current)
        {
            CaseItemEnd caseItemEnd = new CaseItemEnd();
            caseItemEnd.node = current;
            cases.Add(caseItemEnd);
        }
    }
    /// <summary>
    /// Demultiplexor
    /// </summary>
    internal class NodeSwitch : Node
    {
        private ValuePromise valuePromise;
        public ValuePromise ValuePromise => valuePromise;
        public readonly List<CaseItem> cases = new List<CaseItem>();
        public readonly CaseItem defaultCase = new CaseItem();
        public NodeSwitch() : base()
        {
            this.typeNode = ENodeType.SwitchBegin;
        }

        internal override void Execute(ReaderContext readerContext)
        {
            for (int i = 0; i < cases.Count; i++)
            {
                var caseItem = cases[i];
                if (caseItem.Compare(valuePromise.GetValue(readerContext)))
                {
                    this.Tail = caseItem.node;
                    return;
                }
            }
            this.Tail = defaultCase.node ?? throw new NullReferenceException("Default Node Must be not empty");
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            Execute(readerContext);
            return Task.CompletedTask;
        }

        internal override ReadOnlyMemory<byte> GetResultStripped()
        {
            return default;
        }

        internal void AddCaseValue(ReadOnlyMemory<byte> bytes, NodeSwitchItem node)
        {
            CaseItem caseItem = new CaseItem();
            caseItem.data = bytes;
            caseItem.node = node;
            cases.Add(caseItem);
        }
        internal void AddCaseDefault(NodeSwitchItem node)
        {
            if (cases.Contains(defaultCase)) throw new ArgumentException($"Default case is already added");
            defaultCase.node = node;
            cases.Add(defaultCase);
        }
        internal void AddValueSource(ValuePromise valuePromise)
        {
            this.valuePromise = valuePromise;
        }
        internal void AddValueSourceForCase(ValuePromise valuePromise)
        {
            throw new System.NotImplementedException();
        }

    }
    internal class CaseItemEnd
    {
        public Node node;
    }

    internal class CaseItem
    {
        public ReadOnlyMemory<byte> data;
        public NodeSwitchItem node;
        internal bool Compare(ReadOnlyMemory<byte> readOnlyMemory)
        {
            return data.CompareWith(in readOnlyMemory);
        }
    }

    internal class NodeSwitchItem : Node
    {
        public NodeSwitchItem() : base()
        {
            this.typeNode = ENodeType.SwitchItemBegin;
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
            throw new NotImplementedException();
        }
    }
    internal class NodeSwitchItemEnd : Node
    {
        public NodeSwitchItemEnd() : base()
        {
            this.typeNode = ENodeType.SwitchItemEnd;
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
            throw new NotImplementedException();
        }


    }
}