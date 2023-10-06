using System;

namespace IziHardGames.Libs.ForHttp11.Maps
{
    public struct EnumerabableFromBufferForHttp11
    {
        private ReadOnlyMemory<byte> memory;

        public EnumerabableFromBufferForHttp11(in ReadOnlyMemory<byte> memory)
        {
            this.memory = memory;
        }
        public EnumeratorFromBufferForHttp11 GetEnumerator()
        {
            return new EnumeratorFromBufferForHttp11(memory);
        }
    }

    public struct EnumeratorFromBufferForHttp11
    {
        private ReadOnlyMemory<byte> memory;
        public MapMessageHttp11 Current { get; set; }

        public EnumeratorFromBufferForHttp11(in ReadOnlyMemory<byte> memory) : this()
        {
            this.memory = memory;
        }

        public bool MoveNext()
        {
            int lengthBody = default;
            int lengthHeaders = memory.IndexAfterRnRn();
            if (lengthHeaders < 0)
            {
                return false;
            }
            int length = lengthHeaders + lengthBody;
            //decrease scanning region by exluding scanned segment
            memory = memory.Slice(length);
            return true;
        }
    }
}
