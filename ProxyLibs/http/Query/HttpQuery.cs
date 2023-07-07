using HttpDecodingProxy.ForHttp;
using ProxyLibs.Extensions;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Query = IziHardGames.Libs.ForHttp.Streaming.HttpQuery<IziHardGames.Libs.ForHttp.Streaming.WrapIndexerForStringBuilder>;

namespace IziHardGames.Libs.ForHttp.Streaming
{
    /// <summary>
    /// Stream Query
    /// </summary>
    public ref struct HttpQuery<T> where T : IIndexer<char>
    {
        private T wrap;
        public int offset;
        public int length;

        public EnumForHttpFields Fields => GetEnumerator();
        public object FieldNames => throw new System.NotImplementedException();
        public object FieldValues => throw new System.NotImplementedException();
        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#message.format
        /// </summary>
        public HttpFieldMap<T> StartLine => Fields.First();

        public HttpQuery(T wrap) : this()
        {
            this.wrap = wrap;
        }

        public void Calculate()
        {
            var offset = wrap.Offset;

            int end = wrap.Length - 3;

            for (int i = offset; i < end; i++)
            {
                if (this.wrap[i] == '\r' && this.wrap[i + 1] == '\n' && this.wrap[i + 2] == '\r' && this.wrap[i + 3] == '\n')
                {
                    this.length = i - offset;
                    goto PASSED;
                }
            }
            throw new ArgumentOutOfRangeException($"Can't detect end of the headers part");

            PASSED:
            {

            }
            throw new System.NotImplementedException();
        }

        public EnumForHttpFields GetEnumerator()
        {
            return new EnumForHttpFields(wrap);
        }
        public HttpFieldMap<T> Header(string startSubstring)
        {
            foreach (var field in this)
            {
                if (field.StartWith(startSubstring))
                {
                    return field;
                }
            }
            throw new ArgumentOutOfRangeException($"Not Founded startSubstring:{startSubstring}");
        }
        public static HttpQuery<T> From(T wrap)
        {
            return new HttpQuery<T>(wrap);
        }

        public static void Test()
        {
            StringBuilder sb = new StringBuilder();

            var headers = new Query(new WrapIndexerForStringBuilder(sb, 0, 0));

            var values = headers.Fields;

            foreach (var header in headers)
            {
                foreach (var value in header)
                {
                    foreach (var charr in value)
                    {

                    }
                }
            }
        }

        public struct EnumForHttpFields
        {
            private T indexer;
            private int offset;
            private int length;

            public HttpFieldMap<T> Current { get; private set; }

            public EnumForHttpFields(T sb) : this()
            {
                this.indexer = sb;
                this.offset = sb.Offset;
            }

            public HttpFieldMap<T>.EnumForHttpFieldValues GetEnumerator()
            {
                throw new System.NotImplementedException();
                return new HttpFieldMap<T>.EnumForHttpFieldValues(indexer, default, default);
            }

            public bool MoveNext()
            {
                int end = offset + this.length;

                if (indexer.TryFindLineLength(offset, out int headerLength))
                {

                }
                throw new System.NotImplementedException();
            }

            public HttpFieldMap<T> GetHeaderByIndex(int index)
            {
#if DEBUG
                if (index < 0) throw new ArgumentOutOfRangeException("Index is below zero");
#endif
                int offset = default;
                int offsetPrevious = default;
                int length = -1;

                for (int i = 0; i <= index; i++)
                {
                    offsetPrevious = offset;

                    if (indexer.TryFindLineLength(offset, out length))
                    {
                        offset += length + 2;
                    }
                    else
                    {
                        throw new ArgumentException($"Can't Find New Line");
                    }
                }
                return new HttpFieldMap<T>(indexer, offsetPrevious, length);
            }

            public HttpFieldMap<T> First()
            {
                throw new NotImplementedException();
            }
            public HttpFieldMap<T> Last()
            {
                throw new NotImplementedException();
            }
            public int Count()
            {
                throw new System.NotImplementedException();
            }
        }
    }
}

