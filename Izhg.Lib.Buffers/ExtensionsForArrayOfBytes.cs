namespace System
{
    public static class ExtensionsForArrayOfBytes
    {
        public static string ToStringUtf8(this byte[] array)
        {
            return new ReadOnlySpan<byte>(array).ToStringUtf8();
        }
    }
}