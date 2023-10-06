using System.Collections.Generic;
using System.Runtime.CompilerServices;
using IziHardGames.Lib.Collections.Contracts;

namespace IziHardGames.MappedFrameReader
{
    /// <summary>
    /// Note: in case of conditional read this reader read Value from specific storage and perform comparison. Based on result it changes <see cref="Next"/>
    /// </summary>
    internal class ReadOperation : ILinkedList<ReadOperation>
    {
        /// <summary>
        /// Next Read Operation Based On Scheme
        /// </summary>
        public ReadOperation? Next { get; set; }
        public ReadOperation? Current { get; set; }
        private int length;
        private int index;
        private int groupe;
        /// <summary>
        /// Read value and write it into buffer
        /// </summary>
        private bool isReadLinkedVariable;
        public string? idFrame;

        public ReadOperation[]? conditionalReadOperations;
        public VariableResultStorage variableStorage;

        public void SetReadLength(int length)
        {
            this.length = length;
        }

        public async Task ReadVariableAsync(TaskAwaiter awaiter)
        {
            throw new System.NotImplementedException();
        }
        public async Task ReadAsync()
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Unconditional Read for specific length
        /// </summary>
        /// <param name="length"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Read(int length)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Read Byte And Check
        /// </summary>
        /// <param name="condition"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ReadCompare(Func<bool> condition)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Read Given Count Of Bytes And Perform Check
        /// </summary>
        /// <param name="lengthForward"></param>
        /// <param name="condition"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ReadCompare(int lengthForward, Func<bool> condition)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Set Cycle. Cycled read. Need to set condition To stop.
        /// </summary>
        /// <param name="readOperation"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal void ReturnTo(ReadOperation readOperation)
        {
            this.Next = readOperation;
        }
        internal void SetNext(ReadOperation readOperation)
        {
            this.Next = readOperation;
            readOperation.SetIndex(index + 1);
        }

        private void SetIndex(int index)
        {
            if (this.index != default) throw new InvalidOperationException("index is already set");
            this.index = index;
        }
        public object ToGraph() => throw new System.NotImplementedException();

        internal void FromLinked()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Add Condition To Check right after value is reaed
        /// </summary>
        /// <param name="valuePromise"></param>
        /// <exception cref="NotImplementedException"></exception>
        internal void AddCondition(ValuePromise valuePromise)
        {
            throw new NotImplementedException();
        }

        internal void AsHead()
        {
            SetIndex(0);
        }

        public ReadOperation GetEnumerator()
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