using System;
using System.Buffers;

namespace IziHardGames.Libs.Buffers.Vectors
{
    /// <summary>
    /// Using <see cref="System.Buffers.ArrayPool{T}"/><br/>
    /// <see cref="ArraySegment{T}"/>.
    /// </summary>
    /// Be careful of Disposing of copy: each copy will link to the same array. There might be double disposing
    public readonly struct ArraySegmentDisposable : IDisposable
    {
        public readonly byte[] array;
        public readonly int offset;
        public readonly int length;

        public ArraySegmentDisposable(int size)
        {
            offset = default;
            array = ArrayPool<byte>.Shared.Rent(size);
            length = size;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }
}