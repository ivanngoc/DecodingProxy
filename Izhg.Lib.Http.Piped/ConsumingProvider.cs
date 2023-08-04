using HttpDecodingProxy.ForHttp;
using System;
using Callback = System.Func<IziHardGames.Proxy.Consuming.DataSource, System.Buffers.ReadOnlySequence<byte>, System.Threading.Tasks.Task>;

namespace IziHardGames.Proxy.Consuming
{
    public class ConsumingProvider
    {
        public Callback consumeRequest;
        public Callback consumeResponse;

        public Action<DataSource, HttpObject> consumeRequestMsg;
        public Action<DataSource, HttpObject> consumeResponseMsg;

        public Action<DataSource, HttpBinaryMapped> consumeBinaryRequest;
        public Action<DataSource, HttpBinaryMapped> consumeBinaryResponse;
    }
}