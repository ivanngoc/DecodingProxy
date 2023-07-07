using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Datas;
using IziHardGames.Proxy.Sniffing.ForHttp;
using System.Collections.Concurrent;
using HttpResult = HttpDecodingProxy.ForHttp.HttpObject;

namespace IziHardGames.Proxy.Recoreder
{
    public class BlockRecordingSource : IDisposable
    {
        public uint id;
        private ConcurrentDictionary<int, Block> items = new ConcurrentDictionary<int, Block>();//(-1, 8);
        private int expectedSeqNumber;
        private readonly PipedHttpStream httpStream = new PipedHttpStream();

        internal BlockRecordingSource Init(uint id, Action<DataSource, HttpResult> action)
        {
            this.id = id;
            httpStream.OnObject += action;
            return this;
        }

        internal void Push(Block data)
        {
            lock (this)
            {
                if (expectedSeqNumber == data.SequenceNumber)
                {   // CPU-bound
                    httpStream.Write(data.Data, 0, data.length);
                    expectedSeqNumber++;
                    data.Unuse();
                }
                else
                {
                    var value = items.GetOrAdd(data.SequenceNumber, (key) => data);
                    value.Use();
                }
                FindContinuation();
            }

            void FindContinuation()
            {
                while (items.TryRemove(expectedSeqNumber, out var val))
                {
                    httpStream.Write(val.Data, 0, val.length);
                    val.Unuse();
                    val.TryDispose();
                    expectedSeqNumber++;
                }
            }
        }


        public void Dispose()
        {
            id = default;
            expectedSeqNumber = default;

            items.Clear();
            PoolObjectsConcurent<BlockRecordingSource>.Shared.Return(this);
            httpStream.Dispose();
        }
    }
}