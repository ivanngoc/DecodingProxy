namespace IziHardGames.Lib.Collections.Contracts
{
    public interface IDoubleEdgeList<T> : INodeBidirectional<T>
    {


    }
    public interface ILinkedList<T>
    {
        public T Next { get; set; }
    }

    public interface INodeBidirectional<T>: INodeForward<T>, INodeBackward<T>
    {
    }

    public interface INodeForward<T>
    {
        public T Tail { get; set; }
    }
    public interface INodeBackward<T>
    {
        public T Head { get; set; }
    }
}
