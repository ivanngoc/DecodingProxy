using System.Runtime.CompilerServices;
using System.Text;

namespace System
{

    public static class ExtensionsForReadOnlySpanAsString
    {

        /// <summary>
        /// Index Of sequence \r\n\r\n. Usualy used in HTTP for separate headers from body
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexAfterRnRn(in this ReadOnlySpan<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == '\r' && span[i + 1] == '\n' && span[i + 2] == '\r' && span[i + 3] == '\n')
                {
                    return i + 4;
                }
            }
            return -1;
        }
        /// <summary>
        /// Index after given sibstring
        /// Example: index of "aka" in "rubakan" = 6;
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexAfterSubstring(in this ReadOnlySpan<byte> span, in ReadOnlySpan<char> substring)
        {
            throw new System.NotImplementedException();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOfAnyWhiteSpaceOrNewLine(in this ReadOnlySpan<byte> span)
        {
            for (int i = 0; i < span.Length; i++)
            {
                if (char.IsWhiteSpace((char)span[i]) || span[i] == '\r' || span[i] == '\n') return i;
            }
            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringHex(in this ReadOnlySpan<byte> span)
        {
            Span<char> chars = stackalloc char[span.Length << 1];
            int k = default;
            for (int i = 0; i < span.Length; i++, k += 2)
            {
                var v = ParseByte.ByteToHex(span[i]);
                chars[k] = v.Item1;
                chars[k + 1] = v.Item2;
            }
            return new string(chars);
        }
        public static string ToStringUtf8(in this ReadOnlySpan<byte> span)
        {
            return Encoding.UTF8.GetString(span);
        }
        /// <summary>
        /// Convert each bytes to char (only numeric utf-8, 1 byte length)  than parse it
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static int ParseToInt32(in this ReadOnlySpan<byte> span)
        {
            Span<char> chars = stackalloc char[span.Length];
            for (int i = 0; i < span.Length; i++)
            {
                chars[i] = (char)span[i];
            }
            return int.Parse(chars);
        }
        public static void CopyAsChar(in this ReadOnlySpan<byte> span, ref Span<char> result)
        {
            if (span.Length != result.Length) throw new ArgumentException($"Spans must be same length");
            for (int i = 0; i < span.Length; i++)
            {
                result[i] = (char)span[i];
            }
        }
        public static bool CompareWith(in this ReadOnlySpan<byte> span, in ReadOnlySpan<char> substring)
        {
            if (span.Length != substring.Length) return false;

            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != substring[i]) return false;
            }
            return true;
        }
    }
}