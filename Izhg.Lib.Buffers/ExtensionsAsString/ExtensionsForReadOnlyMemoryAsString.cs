using System.Runtime.CompilerServices;
using static System.Text.Encoding;

namespace System
{

    public static class ExtensionsForReadOnlyMemoryAsString
    {
        public static bool IsContainDigit(in this ReadOnlyMemory<byte> mem)
        {
            var span = mem.Span;
            for (int i = 0; i < span.Length; i++)
            {
                if (char.IsDigit((char)span[i])) return true;
            }
            return false;
        }

        /// <inheritdoc cref="ExtensionsForReadOnlySpanAsString.IndexAfterRnRn(in ReadOnlySpan{byte})"/>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexAfterRnRn(in this ReadOnlyMemory<byte> mem)
        {
            return mem.Span.IndexAfterRnRn();
        }

        /// <inheritdoc cref="ExtensionsForReadOnlySpanAsString.IndexAfterSubstring(in ReadOnlySpan{byte}, in ReadOnlySpan{char})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexAfterSubstring(in this ReadOnlyMemory<byte> mem, in ReadOnlySpan<char> chars)
        {
            return mem.Span.IndexAfterSubstring(in chars);
        }
        public static int IndexOf(in this ReadOnlyMemory<byte> mem, byte c)
        {
            return mem.Span.IndexOf(c);
        }
        public static int IndexOf(in this ReadOnlyMemory<byte> mem, char c)
        {
            return mem.Span.IndexOf((byte)c);
        }
        public static string ToStringHex(in this ReadOnlyMemory<byte> mem)
        {
            return mem.Span.ToStringHex();
        }
        public static string ToStringUtf8(in this ReadOnlyMemory<byte> mem)
        {
            return UTF8.GetString(mem.Span);
        }
        public static bool CompareWith(in this ReadOnlyMemory<byte> left, in ReadOnlyMemory<byte> right)
        {
            var spanLeft = left.Span;
            var spanRight = right.Span;
            return spanLeft.SequenceEqual(spanRight);
        }
        public static bool CompareWith(in this ReadOnlyMemory<byte> mem, in ReadOnlySpan<char> str)
        {
            var span = mem.Span;
            return span.CompareWith(in str);
        }
    }
}