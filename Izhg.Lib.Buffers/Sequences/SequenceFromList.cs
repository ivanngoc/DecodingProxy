using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Buffers.Sequences
{
    /// <summary>
    /// Из-за того что нельзя переопределить свойства Next и Memory пока не придумал как можно использовать только этот объект для движения вперед как псевдо двусвязный список
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SequenceFromList<T> : ReadOnlySequenceSegment<T>
    {
        public List<ReadOnlyMemory<byte>> memories = new List<ReadOnlyMemory<byte>>();
        private int index;
        public SequenceFromList() : base()
        {
            Next = this;
        }
    }
}
