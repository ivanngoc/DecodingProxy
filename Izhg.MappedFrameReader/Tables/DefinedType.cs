namespace IziHardGames.MappedFrameReader
{
    public class DefinedType
    {
        internal string name = string.Empty;
        internal int size;
        internal Scheme.ESizeType sizeType;
        internal ValuePromise? valuePromise;
        internal ValueParser? valueParser;

        internal ReadOnlyMemory<byte> From(string caseValue)
        {
            return valueParser.ParseValueToReadOnlyMemory(caseValue);
        }
    }

    public abstract class ValueParser
    {
        public abstract ReadOnlyMemory<byte> ParseValueToReadOnlyMemory(string input);
    }

    internal class ByteParser : ValueParser
    {
        public override ReadOnlyMemory<byte> ParseValueToReadOnlyMemory(string input)
        {
            return new ReadOnlyMemory<byte>(new byte[] { Convert.ToByte(input, 16) });
        }
    }

    internal class SliceParser : ValueParser
    {
        public override ReadOnlyMemory<byte> ParseValueToReadOnlyMemory(string input)
        {
            throw new NotImplementedException();
        }
    }

    internal class StringParser : ValueParser
    {
        public override ReadOnlyMemory<byte> ParseValueToReadOnlyMemory(string input)
        {
            throw new NotImplementedException();
        }
    }
}