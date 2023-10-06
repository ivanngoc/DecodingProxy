using ProxyLibs.Extensions;
using System.Text;

namespace IziHardGames.Libs.HttpCommon.Streaming
{
    public struct WrapIndexerForStringBuilder : IIndexer<char>
    {
        private StringBuilder sb;
        private int offset;
        private int length;
        public int Length { get => length; }
        public int Offset { get => offset; }
        public char this[int index] { get => sb[index]; }

        public WrapIndexerForStringBuilder(StringBuilder stringBuilder, int offset, int count)
        {
            this.sb = stringBuilder;
            this.offset = offset;
            this.length = offset;
        }

        public bool TryFindLineLength(int offset, out int length)
        {
            return sb.TryFindLineLength(offset, out length);
        }

        public string GetString(int offset, int length)
        {
            return sb.ToString(offset, length);
        }
    }

    public interface IIndexer<T>
    {
        T this[int index] { get; }
        int Length { get; }
        int Offset { get; }
        string GetString(int offset, int length);
        bool TryFindLineLength(int offset, out int length);
    }
}

