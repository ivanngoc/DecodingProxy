using System;
using System.Text;

namespace IziHardGames.Libs.HttpCommon.Streaming
{
    public struct HttpFieldMap<T> where T : IIndexer<char>
    {
        private T indexer;
        private int offset;
        private int length;
        public string Value => indexer.GetString(offset, length);

        public EnumForHttpFieldValues Values => new EnumForHttpFieldValues();

        public HttpFieldMap(T sb, int offset, int length)
        {
            this.indexer = sb;
            this.offset = offset;
            this.length = length;
        }

        public EnumForHttpFieldValues GetEnumerator()
        {
            return new EnumForHttpFieldValues();
        }

        public bool StartWith(string startSubstring)
        {
            for (int i = 0; i < startSubstring.Length; i++)
            {
                if (indexer[i + offset] != startSubstring[i])
                {
                    return false;
                }
            }
            return true;
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public struct EnumForHttpFieldValues
        {
            private T wrap;
            private int offset;
            private int length;
            private int index;
            public HttpFieldValues<T> Current { get; private set; }

            public EnumForHttpFieldValues(T sb, int offset, int length)
            {
                this.wrap = sb;
                this.offset = offset;
                this.length = length;
                this.index = -1;
                this.Current = default;
            }

            public HttpFieldValues<T> First()
            {
                throw new System.NotImplementedException();
            }
            public HttpFieldValues<T> Last()
            {
                throw new System.NotImplementedException();
            }

            public int Count()
            {
                throw new System.NotImplementedException();
            }

            public bool MoveNext()
            {
                throw new System.NotImplementedException();
            }
        }

    }

}