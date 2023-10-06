using IziHardGames.Lib.Collections.Contracts;
using static IziHardGames.MappedFrameReader.SchemeImporter;
namespace IziHardGames.MappedFrameReader
{
    internal class ReadFrame : ILinkedList<ReadFrame>
    {
        public ReadFrame Next { get; set; }
        public ReadFrame Current { get; set; }

        public ReadOperation head;
        public string name;
        internal ESourceType sourceType;
        internal EMods mods;

        public void SetNext(ReadFrame readFrame)
        {
            this.Next = readFrame;
        }
        internal void SetHead(ReadOperation roRead)
        {
            this.head = roRead;
        }

        internal ReadFrame Find(string name)
        {
            ReadFrame current = this;
            REPEAT:
            if (current == null) throw new NullReferenceException($"Can't Find ReadFrame with name [{name}]");

            if (current.name == name) return current;
            else
            {
                current = current.Next;
                goto REPEAT;
            }
        }

        public ReadFrame GetEnumerator()
        {
            Current = this;
            return this;
        }

        public bool MoveNext()
        {
            if (Current.Next == null) return false;
            Current = Current.Next;
            return true;
        }
    }
}