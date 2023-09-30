using System.Buffers;

namespace IziHardGames.Proxy.Consuming
{
    public class HttpSource
    {
        public readonly int id;
        public int variant; // #num objets of same type
        public int generation;
        public int deathCount;
        public string title;
        public EHttpConnectionFlags flagsAgent;
        public EHttpConnectionFlags flagsOrigin;
        public string host;
        internal int port;

        public HttpSource(string title)
        {
            this.title = title;
            id = GetHashCode();
        }

        public void StartNewGeneration()
        {
            generation++;
        }
        public void EndGeneration()
        {
            deathCount++;
        }

        public string ToStringInfo()
        {
            return $"Source:{id}. title:{title}. Gen:{generation}. deaths:{deathCount}";
        }

        public void SetFlagsAgent(EHttpConnectionFlags flags)
        {
            this.flagsAgent = flags;
        }
        public void SetFlagsOrigin(EHttpConnectionFlags flags)
        {
            this.flagsOrigin = flags;
        }
    }
}