using System;

namespace IziHardGames.Libs.Text
{
    public static class Chars
    {

        public static bool IsEqualCI(in ReadOnlySpan<byte> buffer, char[] substring)
        {
            if (substring.Length != buffer.Length) return false;
            for (int i = 0; i < substring.Length; i++)
            {
                if (char.ToLowerInvariant((char)buffer[i]) != char.ToLowerInvariant(substring[i])) return false;
            }
            return true;
        }

        public static bool IsEqualCI(in ReadOnlyMemory<byte> mem, char first, char second, char third, char fourth)
        {
            throw new System.NotImplementedException();
        }
        public static bool IsEqualCI(byte left, char right)
        {
            throw new System.NotImplementedException();
        }
    }
}
