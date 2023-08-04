namespace Izhg.Lib.Collections.Contracts
{
    public interface IDoubleEdgeList<T> : IDoubleEdgeNode<T>
    {


    }
    public interface IDoubleEdgeNode<T>
    {
        public T Head { get; set; }
        public T Tail { get; set; }
    }
}
