using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace IziHardGames.Libs.IO
{
    public static class ExtensionsForReadOnlySequenceUtf8
    {
        public unsafe static bool TryGetChar(in this ReadOnlySequence<byte> seq, out char c, out byte size)
        {
            if (seq.Length > 0)
            {
                var span = seq.FirstSpan;
                size = Utf8.GetSize(span[0]);
                if (size > seq.Length) goto END;
                try
                {
                    char result = default;
                    byte* pointer = (byte*)&result;
                    for (var i = 0; i < size; i++)
                    {
                        pointer[i] = span[i];
                    }
                    c = result;
                    return true;
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw ex; // not supported. segment is too small default segment size is 4096. Minimum expected size here 2048
                }
            }
            END:
            c = default;
            size = default;
            return false;
        }
        public static bool IsStartWith(in this ReadOnlySequence<byte> seq, string substring)
        {
            throw new System.NotImplementedException();
        }
        public static bool IsStartWithSingleSize(in this ReadOnlySequence<byte> seq, string substring)
        {
            if (seq.Length < substring.Length) return false;
            int index = default;

            foreach (var seg in seq)
            {
                var span = seg.Span;

                for (int i = 0; i < seg.Length; i++)
                {
                    if ((char)span[i] != substring[index])
                    {
                        return false;
                    }
                    else
                    {
                        index++;
                        if (index == substring.Length) return true;
                    }
                }
            }
            return false;
        }
    }
}
