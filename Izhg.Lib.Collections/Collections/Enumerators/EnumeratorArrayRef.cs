using System.Runtime.CompilerServices;

namespace IziHardGames.Libs.NonEngine.Enumerators
{
    public ref struct EnumeratorArrayRef<T>
    {
        private readonly T[] items;
        private int index;
        private int count;
        public ref T Current { get => ref items[index]; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnumeratorArrayRef(T[] items, int count)
        {
            this.items = items;
            index = -1;
            this.count = count;
        }

        public bool MoveNext()
        {
            index++;
            return index < count;
        }
    }
}