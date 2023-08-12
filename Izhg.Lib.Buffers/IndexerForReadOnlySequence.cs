using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace IziHardGames.Core.Buffers
{
    public readonly struct IndexerForReadOnlySequence<T> : IIndexReader<T>, IReadOnlySpanProvider<T>
    {
        public T this[int index] { get => GetByIndex(index); }
        public readonly ReadOnlySequence<T> seq;

        public IndexerForReadOnlySequence(ReadOnlySequence<T> seq)
        {
            this.seq = seq;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetByIndex(int index)
        {
            if (index < seq.Length)
            {
                return seq.Slice(index).FirstSpan[0];
            }
            throw new ArgumentOutOfRangeException($"Length:{seq.Length}. Index:{index}");
        }
        public ReadOnlySpan<T> GetSpan(int offset, int length)
        {
            var slice = seq.Slice(offset, length);
            if (slice.FirstSpan.Length < length) throw new NotSupportedException();
            var res = slice.FirstSpan.Slice(0, length);
            return res;
        }
    }
}