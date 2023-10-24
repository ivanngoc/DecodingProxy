using System.Collections;
using IziHardGames.Lib.Collections.Contracts;

namespace IziHardGames.MappedFrameReader
{
    internal struct EnumeratorNodes<T> : IEnumerator<T>
        where T : INodeForward<T>
    {
        private T head;
        public T Current { get; set; }
        object IEnumerator.Current { get => throw new System.NotImplementedException(); }

        public EnumeratorNodes(T node)
        {
            this.head = node;
            Current = node;
        }

        public bool MoveNext()
        {
            if (Current.Tail != null)
            {
                Current = Current.Tail ?? throw new NullReferenceException();
                return true;
            }
            return false;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}