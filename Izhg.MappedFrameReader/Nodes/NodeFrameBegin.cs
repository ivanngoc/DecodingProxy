using IziHardGames.Lib.Collections.Contracts;
using static IziHardGames.MappedFrameReader.SchemeImporter;
namespace IziHardGames.MappedFrameReader
{
    internal class NodeFrameEnd : Node
    {
        public NodeFrameEnd() : base()
        {
            this.typeNode = ENodeType.FrameEnd;
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

    internal class NodeFrameBegin : Node, ILinkedList<NodeFrameBegin>
    {
        public NodeFrameBegin Next { get; set; }
        public NodeFrameBegin Current { get; set; }
        /// <summary>
        /// For Nested Reads
        /// </summary>
        private NodeFrameEnd end;

        public Node head;
        public string name;
        internal ESourceType sourceType;
        internal EMods mods;

        public NodeFrameBegin() : base()
        {

        }

        public void SetEnd(NodeFrameEnd end)
        {
            this.end = end;
        }
        public void SetNext(NodeFrameBegin readFrame)
        {
            this.Next = readFrame;
        }
        internal void SetHead(NodeReadFromSource roRead)
        {
            this.head = roRead;
        }

        internal override void Execute(ReaderContext readerContext)
        {
            throw new NotImplementedException();
        }

        internal override Task ExecuteAsync(ReadOnlyMemory<byte> mem, ReaderContext readerContext)
        {
            return Task.CompletedTask;
        }

        internal void SetRepeat()
        {
            throw new System.NotImplementedException();
        }

        internal override ReadOnlyMemory<byte> GetResultStripped()
        {
            return default;
        }
    }
}