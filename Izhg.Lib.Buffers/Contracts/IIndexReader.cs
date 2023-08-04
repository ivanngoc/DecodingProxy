namespace IziHardGames.Core.Buffers
{

    public interface IIndexReader<T>
    {
        T this[int index] { get; }
    }

    public interface IRangeCopy<T>
    {
        void CopyTo(T destination, int offset, int length);
    }
}