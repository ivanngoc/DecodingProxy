namespace System
{
    public static class ExtensionsForReadOnlySpan
    {
        public static int IndexOfCI(in this ReadOnlySpan<byte> bytes, byte c)
        {
            throw new System.NotImplementedException();
        }
        public static int IndexOfCS(in this ReadOnlySpan<byte> bytes, char c)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == c) return i;
            }
            return -1;
        }
        public static int IndexOfCS(in this ReadOnlySpan<byte> bytes, byte c)
        {
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == c) return i;
            }
            return -1;
        }
    }
}