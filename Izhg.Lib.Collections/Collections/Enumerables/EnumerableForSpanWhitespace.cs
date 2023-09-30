using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.NonEngine.Enumerators;

namespace IziHardGames.Libs.NonEngine.Enumerables
{
    public readonly ref struct EnumerableForSpanWhitespace
    {
        private readonly ReadOnlySpan<byte> spanLine;
        public EnumerableForSpanWhitespace(ReadOnlySpan<byte> spanLine)
        {
            this.spanLine = spanLine;
        }
        public EnumeratorForSpanWhitespace GetEnumerator()
        {
            return new EnumeratorForSpanWhitespace();
        }
    }
}
