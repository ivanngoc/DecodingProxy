using HttpDecodingProxy.ForHttp;
using System;
using Callback = System.Func<IziHardGames.Proxy.Consuming.HttpSource, System.Buffers.ReadOnlySequence<byte>, System.Threading.Tasks.Task>;

namespace IziHardGames.Proxy.Consuming
{
    public class ConsumingProvider
    {
        public Callback consumeRequest;
        public Callback consumeResponse;

        public Action<HttpSource, HttpObject> consumeRequestMsg;
        public Action<HttpSource, HttpObject> consumeResponseMsg;

        public Action<HttpSource, HttpBinaryMapped> consumeBinaryRequest;
        public Action<HttpSource, HttpBinaryMapped> consumeBinaryResponse;
    }
}