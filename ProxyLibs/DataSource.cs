using HttpDecodingProxy.ForHttp;
using System.Buffers;
using Callback = System.Func<IziHardGames.Proxy.Consuming.DataSource, System.Buffers.ReadOnlySequence<byte>, System.Threading.Tasks.Task>;

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

    public class ConsumingProvider
    {
        public IBlockConsumer[] consumersRequest;
        public IBlockConsumer[] consumersResponse;

        public Callback consumeRequest;
        public Callback consumeResponse;

        public Action<DataSource, HttpObject> consumeRequestMsg;
        public Action<DataSource, HttpObject> consumeResponseMsg;

        public Action<DataSource, HttpBinary> consumeBinaryRequest;
        public Action<DataSource, HttpBinary> consumeBinaryResponse;
    }
}