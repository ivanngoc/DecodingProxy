namespace System
{
    public static class ExtensionsForReadOnlyMemoryToStructs
    {
        /// <summary>
        /// UTF-8 byte representation to Int32
        /// </summary>
        /// <param name="mem"></param>
        /// <returns></returns>
        public static int ToInt32(in this ReadOnlyMemory<byte> mem)
        {
            var span = mem.Span;
            Span<char> chars = stackalloc char[mem.Length];
            for (int i = 0; i < mem.Length; i++)
            {
                chars[i] = (char)span[i];
            }
            return int.Parse(chars);
        }
    }
}