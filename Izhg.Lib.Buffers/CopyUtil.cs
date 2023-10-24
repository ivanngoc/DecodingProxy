using System;
using System.Runtime.CompilerServices;

namespace IziHardGames.Core.Buffers
{
    public static class CopyUtil
    {
        #region From ReadOnlySpan
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy<T>(in ReadOnlySpan<T> from, in Span<T> to)
        {
            from.CopyTo(to);
            return from.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy(in ReadOnlySpan<byte> span, byte[] buffer, int offset)
        {
            Span<byte> target = new Span<byte>(buffer, offset, buffer.Length - offset);
            span.CopyTo(target);
            return span.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy(in ReadOnlySpan<byte> from, int fromOffset, int fromLength, byte[] to, int toOffset, int toCount)
        {
            int lengthToCopy = toCount > fromLength ? fromLength : toCount;
            var span = from.Slice(fromOffset, lengthToCopy);
            Span<byte> target = new Span<byte>(to, toOffset, lengthToCopy);
            span.CopyTo(target);
            return lengthToCopy;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy(in ReadOnlySpan<byte> from, int fromOffset, int fromLength, in Span<byte> to)
        {
            int toCount = to.Length;
            int lengthToCopy = toCount > fromLength ? fromLength : toCount;
            var source = from.Slice(fromOffset, lengthToCopy);
            source.CopyTo(to);
            return lengthToCopy;
        }
        #endregion





        #region From ReadOnlyMemory
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy(in ReadOnlyMemory<byte> source, int fromOffset, int fromLength, byte[] buffer, int toOffset, int toCount)
        {
            var span = source.Span;
            return Copy(in span, fromOffset, fromLength, buffer, toOffset, toCount);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Copy(in ReadOnlyMemory<byte> from, int fromOffset, int fromLength, in Span<byte> to)
        {
            var span = from.Span;
            return Copy(in span, fromOffset, fromLength, in to);
        } 
        #endregion
    }
}