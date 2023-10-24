using System.Text;

namespace IziHardGames.Libs.NonEngine.Enumerators
{
    /// <summary>
    /// Split Span by "\r\n". "\r\n" Not Included in any splits
    /// </summary>
    public ref struct EnumeratorForSpanLine
    {
        private ReadOnlyMemory<byte> source;
        public ReadOnlyMemory<byte> Current { get; set; }
        public int counter;
        private int offset;
        private int start;
        private int end;
#if DEBUG
        public string DebugCurrent => Encoding.UTF8.GetString(Current.Span);
#endif

        public EnumeratorForSpanLine(in ReadOnlyMemory<byte> source) : this()
        {
            this.source = source;
            counter = -1;
            start = 0;
            end = source.Length - 1;
        }

        public bool MoveNext()
        {
            for (; offset < end; offset++)
            {
                if ((char)source.Span[offset] == '\r' && (char)source.Span[offset + 1] == '\n')
                {
                    Current = source.Slice(start, offset - start);
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