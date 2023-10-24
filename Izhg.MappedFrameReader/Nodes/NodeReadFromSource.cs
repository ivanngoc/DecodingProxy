using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IziHardGames.Lib.Collections.Contracts;

namespace IziHardGames.MappedFrameReader
{
    /// <summary>
    /// Read From Linked variable
    /// Note: in case of conditional read this reader read Value from specific storage and perform comparison. Based on result it changes <see cref="Next"/>
    /// </summary>
    internal class NodeReadFromSource : Node, ILinkedList<NodeReadFromSource>, IAdvancingNode
    {
        /// <summary>
        /// Next Read Operation Based On Scheme
        /// </summary>
        public NodeReadFromSource? Next { get; set; }
        public NodeReadFromSource? Current { get; set; }


        private string idName;
        private int index;
        private int groupe;
        /// <summary>
        /// Read value and write it into buffer
        /// </summary>
        private bool isReadLinkedVariable;
        public string? idFrame;

        public NodeReadFromSource[]? conditionalReadOperations;
        public VariableResultStorage? variableStorage;


        private readonly Action actionReadFromPromise;

        public NodeReadFromSource() : base()
        {
            actionReadFromPromise = ReadFromPromise;
            this.typeNode = ENodeType.ReadFromSource;
        }

        private void ReadFromPromise()
        {
            throw new NotImplementedException();
        }

        internal override void Execute(ReaderContext readerContext)
        {
            actionExecute!();
        }
        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            if (mem.Length < lengthToRead) return taskNotEnough;
            var slice = mem.Slice(0, lengthToRead);
            SetResult(slice);
            SetResultStripped(slice);
            lengthConsumed = lengthToRead;
            return base.ExecuteAsync(mem, readerContext);
        }

        private void SetIndex(int index)
        {
            if (this.index != default) throw new InvalidOperationException("index is already set");
            this.index = index;
        }

        public NodeReadFromSource GetEnumerator()
        {
            Current = this;
            return this;
        }

        public bool MoveNext()
        {
            if (Current!.Next == null) return false;
            Current = Current.Next;
            return true;
        }
    }
}