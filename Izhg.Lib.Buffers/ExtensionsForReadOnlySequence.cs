using System.Buffers;
using System.Text;

namespace System
{
    public static class ExtensionsForReadOnlySequence
    {
        public static void CopyToSafe(in this ReadOnlySequence<byte> seq, Span<byte> span)
        {
            int offset = 0;

            foreach (var seg in seq)
            {
                seg.Span.CopyTo(span.Slice(offset, seg.Length));
                offset += seg.Length;
            }
        }
        public static string ToStringHex(in this ReadOnlySequence<byte> seq)
        {
            if (seq.Length == 0) return string.Empty;

            Span<char> chars = stackalloc char[(int)seq.Length << 1];
            int i = default;
            foreach (var seg in seq)
            {
                var span = seg.Span;
                for (int j = 0; j < span.Length; j++, i += 2)
                {
                    var p = ParseByte.ByteToHex(span[j]);
                    chars[i] = p.Item1;
                    chars[i + 1] = p.Item2;
                }
            }
            return new string(chars);
        }
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
}