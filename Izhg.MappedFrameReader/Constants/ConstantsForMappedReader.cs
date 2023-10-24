namespace IziHardGames.MappedFrameReader
{
    internal class ConstantsForMappedReader
    {
        public const string TYPE_BOOL = "bool";
        public const string TYPE_STRING = "string";
        public const string TYPE_INT = "int";
        public const string RESERVED_ID_HEAD = "head";
        public const string KEYWORD_VECTOR = "vector";
        public const string ATR_ALGO_STRING_COMPARE = "StringCompare";
        public const string ATR_ALGO_FUNC = "Func";


        public static readonly string[] PredefinedTypes = new string[]
        {
            "byte",
            "ushort",
            "uint",
            "vector",
            TYPE_STRING,
        };
    }
}