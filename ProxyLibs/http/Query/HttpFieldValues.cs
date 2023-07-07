using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace IziHardGames.Libs.ForHttp.Streaming
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9110.html#fields.values
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct HttpFieldValues<T> where T : IIndexer<char>
    {
        private T wrap;
        private int offset;
        private int length;

        public HttpFieldValues(T wrap, int offset, int length)
        {
            this.wrap = wrap;
            this.offset = offset;
            this.length = length;
        }

        public EnumForValueChars GetEnumerator()
        {
            return new EnumForValueChars();
        }

        public EnumForValueChars GetValueByIndex(int index)
        {
            int end = length + offset;
            int offsetResult = -1;
            int lengthResult = -1;

            for (int i = offset; i < end; i++)
            {

            }
            throw new System.NotImplementedException();

            return new EnumForValueChars(wrap, offsetResult, lengthResult);
        }

        public bool Is(string value)
        {
            throw new NotImplementedException();
        }

        public struct EnumForValueChars
        {
            private T sb;
            private int offset;
            private int length;
            private int position;
            public char Current => sb[position];
            public char this[int index] => sb[index];

            public EnumForValueChars(T sb, int offset, int length)
            {
                this.offset = offset;
                this.length = length;
                this.position = 0;
                this.sb = sb;
            }

            public bool MoveNext()
            {
                throw new System.NotImplementedException();
            }

            public void CopyTo(Span<char> output)
            {
                int end = offset + length;
                for (int i = offset; i < end; i++)
                {
                    output[i] = sb[i];
                }
            }

            public override bool Equals([NotNullWhen(true)] object? obj)
            {
                throw new NotImplementedException();
            }
            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }
            public static bool operator ==(string s, EnumForValueChars num)
            {
                if (s.Length != num.length) return false;

                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] != num[i]) return false;
                }
                return true;
            }

            public static bool operator !=(string s, EnumForValueChars num)
            {
                if (s.Length != num.length) return true;

                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] != num[i]) return true;
                }
                return false;
            }
            public static bool operator ==(EnumForValueChars num, string s)
            {
                if (s.Length != num.length) return false;

                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] != num[i]) return false;
                }
                return true;
            }

            public static bool operator !=(EnumForValueChars num, string s)
            {
                if (s.Length != num.length) return true;

                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] != num[i]) return true;
                }
                return false;
            }

        }

    }
}