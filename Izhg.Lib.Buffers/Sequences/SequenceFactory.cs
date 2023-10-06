using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;

namespace IziHardGames.Libs.Buffers.Sequences
{
    public static class SequenceFactory
    {
        public static ReadOnlySequence<T> FromEnumerable<T>(IEnumerable<ReadOnlyMemory<T>> values)
        {
            BufferSegmentDefault<T> head = BufferSegmentDefault<T>.Rent();
            var firstMem = values.First();
            head.Initilize(firstMem);
            int length = 0;
            int endIndex = 0;

            BufferSegmentDefault<T> tail = head;


            foreach (var mem in values.Skip(1))
            {
                BufferSegmentDefault<T> next = BufferSegmentDefault<T>.Rent();
                next.Initilize(mem);
                tail.SetNext(next);
                length += tail.Memory.Length;
                next.SetEnd(length);
                tail = next;
                endIndex++;
            }
            int totlaLength = length + tail.Memory.Length - 1;
            var lastIndex = tail.Memory.Length;
            return new ReadOnlySequence<T>(head, 0, tail, lastIndex);
        }
    }
}
