using System.Text;

namespace IziHardGames.Libs.NonEngine.Enumerators
{
    /// <summary>
    /// Enumerator skip whitespace chars (not new line like \r\n)
    /// founded whitespace not included in result
    /// if there is sequence of whitespace than there would be length=0 split
    /// </summary>
    public ref struct EnumeratorForSpanWhitespace
    {
        private ReadOnlyMemory<byte> spanLine;
        public ReadOnlyMemory<byte> Current { get; set; }
        public int counter;
        private int offset;
        private int start;
#if DEBUG
        public string DebugCurrent => Encoding.UTF8.GetString(Current.Span);
#endif

        public EnumeratorForSpanWhitespace(ReadOnlyMemory<byte> spanLine) : this()
        {
            this.spanLine = spanLine;
            counter = -1;
            start = 0;
        }

        public bool MoveNext()
        {
            for (; offset < spanLine.Length; offset++)
            {
                if (char.IsWhiteSpace((char)spanLine.Span[offset]))
                {
                    Current = spanLine.Slice(start, offset - start);
                    start = offset + 1;
                    counter++;
                    offset++;
                    return true;
                }
            }
            return false;
        }
    }
}