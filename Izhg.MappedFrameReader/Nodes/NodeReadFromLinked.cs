namespace IziHardGames.MappedFrameReader
{
    internal class NodeReadFromLinked : Node
    {
        private ValuePromise? valuePromise;
        private Node? target;

        public NodeReadFromLinked() : base()
        {
            this.typeNode = ENodeType.ReadFromLinkedValue;
        }

        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            var res = valuePromise!.GetValue(readerContext);
            target!.SetResult(res);
            return taskSucceed;
        }

        #region Scheduling
        internal void ScheduleReadFromPromise(ValuePromise valuePromise)
        {
            this.valuePromise = valuePromise;
        }
        internal void ScheduleReadFromPromise(ValuePromise valuePromise, ref Node node)
        {
            ScheduleReadFromPromise(valuePromise);
            this.target = node;
            SetThisAfter(ref node!);
        }
        /// <summary>
        /// Unconditional Read for specific length
        /// </summary>
        /// <param name="length"></param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ScheduleRead(int length)
        {
            throw new System.NotImplementedException();
        }

        internal override ReadOnlyMemory<byte> GetResultStripped()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}