using System.Text;

namespace IziHardGames.Libs.NonEngine.Enumerators
{
    /// <summary>
    /// Split Span by "\r\n". "\r\n" Not Included in any splits
    /// </summary>
    public ref struct EnumeratorForSpanNewLine
    {
        private ReadOnlyMemory<byte> spanLine;
        public ReadOnlyMemory<byte> Current { get; set; }
        public int counter;
        private int offset;
        private int start;
        private int end;
#if DEBUG
        public string DebugCurrent => Encoding.UTF8.GetString(Current.Span);
#endif

        public EnumeratorForSpanNewLine(in ReadOnlyMemory<byte> spanLine) : this()
        {
            this.spanLine = spanLine;
            counter = -1;
            start = 0;
            end = spanLine.Length - 1;
        }

        public bool MoveNext()
        {
            for (; offset < end; offset++)
            {
                if ((char)spanLine.Span[offset] == '\r' && (char)spanLine.Span[offset + 1] == '\n')
                {
                    Current = spanLine.Slice(start, offset - start);
                    start = offset + 2;
                    counter++;
                    offset++;
                    return true;
                }
            }
            return false;
        }
    }
}