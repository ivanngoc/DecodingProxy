using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using static System.Text.Encoding;

namespace System
{
    public static class ExtensionsForMemory
    {
        public static string ToStringUtf8(this Memory<byte> mem)
        {
            return UTF8.GetString(mem.Span);
        }
    }

    public static class ExtensionsForReadOnlySequence
    {
        public static string ToStringUtf8(in this ReadOnlySequence<byte> seq)
        {
            return Encoding.UTF8.GetString(seq);
        }
        public static void PrintToConsole(in this ReadOnlySequence<byte> seq)
        {
            Console.WriteLine(seq.ToStringUtf8());
        }
        public static int ParseHexWithEncoding(in this ReadOnlySequence<byte> seq)
        {
            string s = default;
            try
            {
                s = Encoding.UTF8.GetString(seq);
                int size = int.Parse(s, System.Globalization.NumberStyles.HexNumber);
                return size;
            }
            catch (FormatException ex)
            {
                throw ex;
            }

        }
        public static int ParseHex(in this ReadOnlySequence<byte> seq)
        {
            try
            {
                Span<char> chars = stackalloc char[(int)seq.Length];
                int offset = default;
                foreach (var segment in seq)
                {
                    var span = segment.Span;
                    for (int i = 0; i < segment.Length; i++, offset++)
                    {
                        chars[offset] = (char)span[i];
                    }
                }
                int size = int.Parse(chars, System.Globalization.NumberStyles.HexNumber);
                return size;
            }
            catch (FormatException ex)
            {
                string s = Encoding.UTF8.GetString(seq);
                throw ex;
            }
        }
        public static int Parse(in this ReadOnlySequence<byte> seq)
        {
            try
            {
                Span<byte> spanBytes = stackalloc byte[(int)seq.Length];
                Span<char> spanChars = stackalloc char[(int)seq.Length];
                seq.CopyTo(spanBytes);

                for (int i = 0; i < spanChars.Length; i++)
                {
                    spanChars[i] = (char)spanBytes[i];
                }
                return int.Parse(spanChars);
            }
            catch (FormatException ex)
            {
#if DEBUG
                string s = Encoding.UTF8.GetString(seq);
#endif
                throw ex;
            }
        }
    }

    public static class ExtensionsForSpan
    {       
        public static string ToStringUtf8(in this Span<byte> span)
        {
            return Encoding.UTF8.GetString(span);
        }
        /// <summary>
        /// Convert each bytes to char (only numeric utf-8, 1 byte length)  than parse it
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static int ParseToInt32(in this Span<byte> span)
        {
            Span<char> chars = stackalloc char[span.Length];
            for (int i = 0; i < span.Length; i++)
            {
                chars[i] = (char)span[i];
            }
            return int.Parse(chars);
        }
        public static void CopyAsChar(in this Span<byte> span, ref Span<char> result)
        {
            if (span.Length != result.Length) throw new ArgumentException($"Spans must be same length");
            for (int i = 0; i < span.Length; i++)
            {
                result[i] = (char)span[i];
            }
        }
        public static bool CompareWith(in this Span<byte> span, string substring)
        {
            return span.CompareWith(substring);
        }

        /// <summary>
        /// Case Insensetive compare
        /// </summary>
        /// <param name="span"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool CompareWithCI(in this Span<byte> span, ReadOnlySpan<char> substring)
        {
            if (span.Length != substring.Length) return false;

            for (int i = 0; i < span.Length; i++)
            {
                if (char.ToLowerInvariant((char)span[i]) != char.ToLowerInvariant(substring[i])) return false;
            }
            return true;
        }
        public static bool CompareWith(in this Span<byte> span, ReadOnlySpan<char> substring)
        {
            if (span.Length != substring.Length) return false;

            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != substring[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Поиск подстроки. Оптимально если потенциально совпадение находится ближе к началу
        /// <br/>
        /// <see cref="ProxyLibs.Extensions.ExtensionsForStringBuilder.RangeContains(Text.StringBuilder, int, int, string)"/>
        /// </summary>
        /// <param name="str"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool GotSubsequenceProbablyAtFront(in this Span<byte> str, ReadOnlySpan<char> substring)
        {
            if (str.Length < substring.Length) return false;
            int endItermediary = str.Length - substring.Length;
            int i = 0, k = 0;

            for (; k < substring.Length; i++)
            {
                if (str[i] != substring[k])
                {
                    if (i > endItermediary)
                    {
                        return false;
                    }
                    k = 0;
                }
                else
                    k++;
            }
            return true;
        }
        /// <summary>
        /// Оптимизирован для ситуаций когда наверняка подстрока будет найдена в конце
        /// </summary>
        /// <param name="src"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool GotSubsequenceProbablyAtBack(in this Span<byte> src, ReadOnlySpan<char> substring)
        {
            if (src.Length < substring.Length) return false;
            int endItermediary = src.Length - substring.Length;
            int i = 0;
            int j = 0;
            // в случае непопадания происходит 3 сравнения. в случае попадания 2. и всегда 2 инкермента
            for (; i < endItermediary; i++)
            {
                for (; j < substring.Length; j++, i++)
                {
                    if (src[i] != substring[j])
                    {
                        j = 0;
                        goto NEXT;
                    }
                }
                return true;
                NEXT: continue;
            }

            for (; i < src.Length; i++, j++)
            {
                if (src[i] != substring[j])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Оптимизирован для ситуаций когда наверняка подстрока будет найдена в конце
        /// </summary>
        /// <param name="src"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool GotSubsequenceProbablyAtBackCI(in this Span<byte> src, ReadOnlySpan<char> substring)
        {
            if (src.Length < substring.Length) return false;
            int endItermediary = src.Length - substring.Length;
            int i = 0;
            int j = 0;
            // в случае непопадания происходит 3 сравнения. в случае попадания 2. и всегда 2 инкермента
            for (; i < endItermediary; i++)
            {
                for (; j < substring.Length; j++, i++)
                {
                    if (char.ToLowerInvariant((char)src[i]) != char.ToLowerInvariant(substring[j]))
                    {
                        j = 0;
                        goto NEXT;
                    }
                }
                return true;
                NEXT: continue;
            }

            for (; i < src.Length; i++, j++)
            {
                if (char.ToLowerInvariant((char)src[i]) != char.ToLowerInvariant(substring[j]))
                {
                    return false;
                }
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> FindSplit(in this Span<byte> span, byte byteChar, int index)
        {
            int offset = 0;
            int count = 0;

            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] == byteChar)
                {
                    if (count == index)
                    {
                        for (; i < span.Length; i++)
                        {
                            if (span[i] == byteChar) return span.Slice(offset, i - offset);
                        }
                        return span.Slice(offset);
                    }
                    else
                    {
                        offset = i;
                    }
                    count++;
                }
            }
            return span;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryFindSplit(in this Span<byte> span, byte byteChar, ref int index, ref int offset, ref int length)
        {
            for (int i = offset; i < span.Length; i++)
            {
                if (byteChar == span[i])
                {
                    index++;
                    length = i - offset;
                    offset = i + 1;
                    return true;
                }
            }
            index++;
            length = span.Length - offset;
            offset = span.Length;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int EndOfSubstringCI(in this Span<byte> span, ReadOnlySpan<char> substring)
        {
            int offset = default;

            for (int i = 0; i < span.Length; i++)
            {
                if (char.ToLowerInvariant((char)span[i]) == char.ToLowerInvariant(substring[offset]))
                {
                    offset++;
                    if (offset == substring.Length) return i;
                }
                else
                {
                    offset = 0;
                }
            }
            return -1;
        }

        public static void Test()
        {
            Test2();

            string b = "ABCDE";
            string a = $"1234567890ABC01234567890{"ABCD"}1234567890ABCDE";

            byte[] aa = Encoding.UTF8.GetBytes(a);
            byte[] ab = Encoding.UTF8.GetBytes(b);

            var res = new Span<byte>(aa).GotSubsequenceProbablyAtFront(b.AsSpan());
            var res2 = new Span<byte>(aa).GotSubsequenceProbablyAtBack(b.AsSpan());
        }
        public static void Test2()
        {
            byte[] b = new byte[(1 << 20) * 2000];
            int last = b.Length - 5;
            b[last + 0] = 1;
            b[last + 1] = 2;
            b[last + 2] = 3;
            b[last + 3] = 4;
            b[last + 4] = 5;

            byte[] r = new byte[] { 0, 1, 2, 3, 4, 5 };
            ReadOnlySpan<char> c = Encoding.UTF8.GetString(r);

            Stopwatch stopwatch = Stopwatch.StartNew();
            // 22723
            var res = new Span<byte>(b).GotSubsequenceProbablyAtFront(c);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            stopwatch.Restart();
            // ~18200
            var res2 = new Span<byte>(b).GotSubsequenceProbablyAtBack(c);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            stopwatch.Stop();
        }

    }
}