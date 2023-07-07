namespace IziHardGames.Libs.NonEngine.Collections
{
    public class SubstringMatchQueue : QueueCircled<byte>
    {
        private readonly string substring;
        public SubstringMatchQueue(string substring) : base(substring.Length)
        {
            this.substring = substring;
        }

        public bool IsMatchOnEnqeue(byte b)
        {
            base.Enqueue(b);

            if (substring.Length != this.Count) return false;

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i] != substring[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}