using System.Buffers;

namespace IziHardGames.MappedFrameReader
{
    /// <summary>
    /// Wrap source of data for using by <see cref="Reader"/>
    /// </summary>
    public abstract class Adapter
    {

    }

    public abstract class AdapterTyped<T> : Adapter
    {
        public T Source;
    }
    public class ArraySource : AdapterTyped<byte[]>
    {
        private byte[] array;
    }
    public class ReadOnlyMemorySource : AdapterTyped<ReadOnlyMemory<byte>>
    {
        public ReadOnlyMemory<byte> source;
    }
    public class ReadOnlySequenceSource : AdapterTyped<ReadOnlySequence<byte>>
    {
        public ReadOnlySequence<byte> source;
    }
}