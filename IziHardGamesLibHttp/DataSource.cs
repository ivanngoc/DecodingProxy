using System.Buffers;

namespace IziHardGames.Proxy.Consuming
{
    public class DataSource
    {
        public readonly int id;
        public int variant; // #num objets of same type
        public int generation;
        public int deathCount;
        public string title;
        public DataSource(string title)
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
    }
}