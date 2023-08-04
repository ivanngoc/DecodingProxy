using System;
using System.Collections;
using IziHardGames.Core;
using IziHardGames.Core.Buffers;

namespace IziHardGames.Proxy.TcpDecoder
{
    public readonly struct IndexReaderForArray<T> : IIndexReader<T>, IRangeCopy<Array>, IArrayProvider<T>, IReadOnlySpanProvider<T>
    {
        public readonly T[] array;
        public T this[int index] { get => array[index]; }
        public IndexReaderForArray(T[] array)
        {
            this.array = array;
        }
        public void CopyTo(Array destination, int offset, int length)
        {
            Array.Copy(array, 0, destination, offset, length);
        }
        public static implicit operator T[](IndexReaderForArray<T> wrap) => wrap.array;
        public static implicit operator IndexReaderForArray<T>(T[] array) => new IndexReaderForArray<T>(array);

        public T[] AsArray()
        {
            return array;
        }

        public ReadOnlySpan<T> GetSpan(int offset, int length)
        {
            return new ReadOnlySpan<T>(array, offset, length);
        }
    }
}