// Last Modified: 2023/04/15 19:03 

using System.Data.SqlTypes;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace ProxyLibs.Extensions
{
    public static class ExtensionsForStringBuilderHttp
    {
        public static bool IsTransferEncodingChunked(this StringBuilder sb, int offset, int count)
        {
            if (
                sb[0] == 'T' &&
                sb[1] == 'r' &&
                sb[2] == 'a' &&
                sb[3] == 'n' &&
                sb[4] == 's' &&
                sb[5] == 'f' &&
                sb[6] == 'e' &&
                sb[7] == 'r' &&
                sb[8] == '-' &&
                sb[9] == 'E' &&
                sb[10] == 'n' &&
                sb[11] == 'c' &&
                sb[12] == 'o' &&
                sb[13] == 'd' &&
                sb[14] == 'i' &&
                sb[15] == 'n' &&
                sb[16] == 'g' &&
                sb[17] == ':' &&
                sb[18] == ' '
                )
            {
                int end = sb.Length - 1;
                for (int i = 19; i < end; i++)
                {
                    if (sb[i] == '\r' && sb[i + 1] == '\n')
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            throw new System.NotImplementedException();
        }
    }

    public static class ExtensionsForStringBuilder
    {
        public static int IndexAfter(this StringBuilder sb, int start, int length, string substring)
        {
            var targetLength = substring.Length;
            if (length < targetLength) return -1;

            int end = start + length;
            int endItermediary = end - targetLength;

            int count = default;
            int i = start;

            for (; i < endItermediary; i++)
            {
                if (sb[i] == substring[count])
                {
                    count++;
                    // greedy check. [i] may exceed [endItermediary]. In that case for succeed it have to be completed after that cycle.
                    for (; count < targetLength; i++)
                    {
                        i++;
                        if (sb[i] != substring[count])
                        {
                            count = 0;
                            goto NEXT;
                        }
                        count++;
                    }
                    return i + 1;
                    NEXT: continue;
                }
                else
                {
                    count = 0;
                }
            }

            // подразумевается что начиная с этого места идет полностью идентичная подстрока. если хотя бы 1 символ в этом регионе отличен то продолжать поиск смысла нет
            if (i > endItermediary) return -1;
            if (sb[i] == substring[count])
            {
                count++;
                // greedy check
                for (; count < targetLength; i++)
                {
                    i++;
                    if (sb[i] != substring[count])
                    {
                        return -1;
                    }
                    count++;
                }
                return i + 1;
            }
            else
            {
                return -1;
            }
        }
        public static int IndexOf(this StringBuilder sb, string substring)
        {
            throw new System.NotImplementedException();
        }


        /// <inheritdoc cref="TryFindLineEnd(StringBuilder, int, int, out int)"/>
        public static bool TryFindLineEnd(this StringBuilder sb, int offset, out int indexLineEndBegin)
        {
            for (int i = offset; i < sb.Length; i++)
            {
                if (sb[i] == '\r')
                {
                    if (sb[i + 1] == '\n')
                    {
                        indexLineEndBegin = i - 1;
                        return true;
                    }
                }
            }
            indexLineEndBegin = -1;
            return false;
        }
        /// <summary>
        /// Give Index before first detected \r\n
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="indexLineEndBegin"></param>
        /// <returns></returns>
        public static bool TryFindLineEnd(this StringBuilder sb, int offset, int count, out int indexLineEndBegin)
        {
            int end = offset + count - 1;

            for (int i = offset; i < end; i++)
            {
                if (sb[i] == '\r')
                {
                    if (sb[i + 1] == '\n')
                    {
                        indexLineEndBegin = i - 1;
                        return true;
                    }
                }
            }
            indexLineEndBegin = -1;
            return false;
        }

        /// <summary>
        /// Move offset to next position after first detected \r\n
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="offset"></param>
        /// <param name="lineLength"></param>
        /// <returns></returns>
        public static bool TryProceedToNextLine(this StringBuilder sb, ref int offset, out int lineLength)
        {
            int end = sb.Length - 1;
            for (int i = offset; i < end; i++)
            {
                if (sb[i] == '\r')
                {
                    if (sb[i + 1] == '\n')
                    {
                        lineLength = i - offset;
                        offset = i + 2;
                        return true;
                    }
                }
            }
            lineLength = -1;
            return false;
        }

        /// <summary>
        /// Find Line length without \r\n
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="offset"></param>
        /// <param name="lineLength"></param>
        /// <returns></returns>
        public static bool TryFindLineLength(this StringBuilder sb, int offset, out int lineLength)
        {
            int end = sb.Length - 1;
            for (int i = offset; i < end; i++)
            {
                if (sb[i] == '\r')
                {
                    if (sb[i + 1] == '\n')
                    {
                        lineLength = i - offset;
                        return true;
                    }
                }
            }
            lineLength = -1;
            return false;
        }


        // worst case: input: "0101010101010101011" need to Find "11" 
        // left - part before [endItermediary] exclusive
        // right - part after [endItermediary] inclusive. Equal length with [value]
        // possible 4 scenario:
        // 1. there is no substring
        // 2. substring is at left part before [endItermediary]
        // 3. substring is in both left and right parts
        /// <summary>
        /// <see cref="ExtensionsForSpan.GotSubsequenceProbablyAtFront(in Span{byte}, ReadOnlySpan{char})"/>
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcStart"></param>
        /// <param name="length"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool RangeContains(this StringBuilder src, int srcStart, int srcLength, string substring)
        {
            if (srcLength < substring.Length) return false;
            int targetEnd = srcStart + srcLength;
            int endItermediary = targetEnd - substring.Length;
            int i = srcStart;
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
        /// 
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="offset">index to start searching</param>
        /// <param name="indexEnclosure">index after \r\n</param>
        /// <param name="lineLength">length of line without \r\n\</param>
        /// <returns></returns>
        public static bool TryItterateLine(this StringBuilder sb, int offset, out int indexEnclosure, out int lineLength)
        {
            throw new System.NotImplementedException();
        }
    }
}