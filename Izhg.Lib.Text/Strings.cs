using System;

namespace IziHardGames.Libs.Text
{

    public static class Strings
    {
        public static bool IsStartWithCI(in ReadOnlySpan<byte> buffer, char[] start)
        {
            if (start.Length > buffer.Length) return false;
            for (int i = 0; i < start.Length; i++)
            {
                if (char.ToLowerInvariant((char)buffer[i]) != char.ToLowerInvariant(start[i])) return false;
            }
            return true;
        }  
        
        public static bool IsStartWithCI(in ReadOnlyMemory<byte> mem, char[] start)
        {
            var buffer = mem.Span;
            if (start.Length > buffer.Length) return false;
            for (int i = 0; i < start.Length; i++)
            {
                if (char.ToLowerInvariant((char)buffer[i]) != char.ToLowerInvariant(start[i])) return false;
            }
            return true;
        }
    }
}
