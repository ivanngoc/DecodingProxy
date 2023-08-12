using System;
using System.Buffers;

namespace IziHardGames.Libs.Buffers.Enumerators
{
    public ref struct EnumerotorForReadOnlySequence
    {
        public readonly ReadOnlySequence<byte> seq;
        private readonly ReadOnlySequence<byte>.Enumerator enumerator;
        private ReadOnlySpan<byte> span;
        public byte Current { get; set; }
        private bool isMemoryPresented;
        private int indexAtSpan = -1;
        private int index = -1;

        public EnumerotorForReadOnlySequence(ReadOnlySequence<byte> seq) : this()
        {
            this.seq = seq;
            this.enumerator = seq.GetEnumerator();
            isMemoryPresented = enumerator.MoveNext();
            span = enumerator.Current.Span;
        }

        public bool MoveNext()
        {
            index++;
            REPEAT:
            if (isMemoryPresented)
            {
                indexAtSpan++;
                if (indexAtSpan < span.Length)
                {
                    Current = span[indexAtSpan];
                    return true;
                }
                else
                {
                    indexAtSpan = -1;
                    isMemoryPresented = enumerator.MoveNext();
                    span = enumerator.Current.Span;
                    goto REPEAT;
                }
            }
            return false;
        }

        /// <summary>
        /// Skip. Like if MoveNext()*count-Times
        /// </summary>
        /// <param name="count"></param>
        /// <returns>
        /// <see langword="true"/> - succesfully advanced<br/>
        /// <see langword="false"/> - can't advance. Current Value and Segment's enumerator remains the same
        /// </returns>
        public bool TryAdvance(int count)
        {
            if (count > 0)
            {
                var left = seq.Length - index + 1;
                if (left > 0)
                {
                    if (count < left)
                    {
                        indexAtSpan += count;
                        Current = span[indexAtSpan];
                        return true;
                    }
                    else
                    {
                        int leftInSegment = default;
                        while (count > 0)
                        {
                            leftInSegment = span.Length - indexAtSpan + 1;
                            count -= leftInSegment;
                            isMemoryPresented = enumerator.MoveNext();
                            span = enumerator.Current.Span;
                            indexAtSpan = -1;
                        }
                        indexAtSpan = span.Length - leftInSegment;
                        Current = span[indexAtSpan];

                    }
                }
                else return false;
            }
            throw new System.NotImplementedException();
        }
    }
}