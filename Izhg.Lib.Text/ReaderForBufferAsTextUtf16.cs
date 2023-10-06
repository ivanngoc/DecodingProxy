using System;
using System.Collections.Generic;
using System.Text;

namespace IziHardGames.Lib.Text
{
    public static class ReaderForBufferAsTextUtf16
    {
        public static int IndexOfWhiteSpace(ReadOnlySpan<byte> span, int offset, int length)
        {
            return IndexOfWhiteSpace(span.Slice(offset, length));
        }
        public static int IndexOfWhiteSpace(ReadOnlySpan<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (char.IsWhiteSpace((char)span[i])) return i;
            }
            return -1;
        }

        public static ReadOnlySpan<byte> ReadLine(ReadOnlyMemory<byte> buffer)
        {
            var span = buffer.Span;
            for (int i = 1; i < buffer.Length; i++)
            {
                if (span[i - 1] == '\r' && span[i] == '\n') return span.Slice(0, i);
            }
            throw new ArgumentOutOfRangeException(@"New Line with \r\n not founded");
        }

        public static bool TryReadLine(ReadOnlyMemory<byte> buffer, out ReadOnlySpan<byte> result)
        {
            var span = buffer.Span;
            return TryReadLine(in span, out result);
        }
        public static bool TryReadLine(in ReadOnlySpan<byte> span, out ReadOnlySpan<byte> result)
        {
            if (span.Length < 2)
            {
                result = default;
                return false;
            }
            for (int i = 1; i < span.Length; i++)
            {
                if (span[i - 1] == '\r' && span[i] == '\n')
                {
                    result = span.Slice(0, i);
                    return true;
                }
            }
            result = default;
            return false;
        }
    }
}
