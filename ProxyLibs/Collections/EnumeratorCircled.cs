using System.Runtime.CompilerServices;

namespace IziHardGames.Libs.NonEngine.Enumerators
{
    public ref struct EnumeratorCircled<T>
    {
        private T[] items;
        private int index;
        private int tail;
        public ref T Current { get => ref items[index]; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnumeratorCircled(T[] items, int head, int tail)
        {
            this.items = items;
            this.tail = tail;
            this.index = head - 1;
        }

        public bool MoveNext()
        {
            index = (index + 1) % items.Length;
            return index != tail;
        }
    }
}