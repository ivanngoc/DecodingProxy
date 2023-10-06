namespace IziHardGames.Lib.Collections.Contracts
{
    public interface IDoubleEdgeList<T> : IDoubleEdgeNode<T>
    {


    }
    public interface ILinkedList<T>
    {
        public T Next { get; set; }
    }

    public interface IDoubleEdgeNode<T>
    {
        public T Head { get; set; }
        public T Tail { get; set; }
    }
}
