using System;
using System.Buffers;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Buffers.Sequences
{
    /// <summary>
    /// Copy of C:\Users\ngoc\Documents\GitHub\runtime\src\libraries\System.IO.Pipelines\src\System\IO\Pipelines\BufferSegment.cs 
    /// </summary>
    internal sealed class BufferSegmentDefault<T> : ReadOnlySequenceSegment<T>, IPoolBind<BufferSegmentDefault<T>>, IDisposable
    {
        private IPoolReturn<BufferSegmentDefault<T>> pool;

        internal static BufferSegmentDefault<T> Rent()
        {
            var pool = PoolObjectsConcurent<BufferSegmentDefault<T>>.Shared;
            var seg = pool.Rent();
            seg.BindToPool(pool);
            return seg;
        }

        public void BindToPool(IPoolReturn<BufferSegmentDefault<T>> pool)
        {
            this.pool = pool;
        }

        public void Dispose()
        {
            this.pool.Return(this);
            pool = default;
            Memory = default;
            Next = default;
            RunningIndex = default;
        }

        internal void Initilize(ReadOnlyMemory<T> mem)
        {
            Memory = mem;
        }

        internal void SetEnd(int length)
        {
            RunningIndex = length;
        }

        internal void SetNext(BufferSegmentDefault<T> next)
        {
            this.Next = next;
        }
    }
}
