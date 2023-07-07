namespace IziHardGames.Libs.Utility
{
    public static class Compare
    {
        public static bool IsEqualContent(Span<byte> left, byte[] right)
        {
            if (left.Length != right.Length) return false;

            for (int i = 0; i < right.Length; i++)
            {
                if (left[i] != right[i]) return false;
            }
            return true;
        }
    }
}
