using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Datas;
using System.Collections.Concurrent;
using HttpResult = HttpDecodingProxy.ForHttp.HttpObject;

namespace IziHardGames.Proxy.Recoreder
{
    public class BlockRecorder : IBlockConsumer
    {
        public ConcurrentDictionary<uint, BlockRecordingSource> pairs = new ConcurrentDictionary<uint, BlockRecordingSource>();// (-1, 128);
        protected Action<DataSource, HttpResult> action;
        public void Consume(Block data)
        {
            var source = pairs.GetOrAdd(data.IdSource, (key) => PoolObjectsConcurent<BlockRecordingSource>.Shared.Rent().Init(key, action));
            source.Push(data);
            data.TryDispose();
        }
    }
}