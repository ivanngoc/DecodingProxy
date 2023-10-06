using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.HttpCommon;
using IziHardGames.Libs.ForHttp20;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Recoreder.MemoryBased;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using HttpResult = HttpDecodingProxy.ForHttp.HttpObject;

namespace IziHardGames.Proxy.Recoreder
{
    public class HttpRecoreder : HttpConsumer
    {
        public readonly RequestMemoryRecorder requestMemoryRecorder = new RequestMemoryRecorder();
        public readonly ResponseMemoryRecorder responseMemoryRecorder = new ResponseMemoryRecorder();
        public const bool isLog = false;
        public static HttpRecoreder Shared { get; set; }
        private static readonly object lockShared = new object();

        private void RecieveRequest(HttpSource source, HttpResult item)
        {
            if (isLog) Console.WriteLine(item.ToStringInfo());
        }
        private void RecieveResponse(HttpSource source, HttpResult item)
        {
            if (isLog) Console.WriteLine(item.ToStringInfo());
        }

        public async Task RecieveResponse(HttpSource dataSource, ReadOnlySequence<byte> data)
        {
            requestMemoryRecorder.Push(dataSource, data);
        }
        public async Task RecieveRequest(HttpSource dataSource, ReadOnlySequence<byte> data)
        {
            requestMemoryRecorder.Push(dataSource, data);
        }

        public void RecieveRequestMsg(HttpSource arg1, HttpResult arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.ToStringInfo()}");
        }
        public void RecieveResponseMsg(HttpSource arg1, HttpResult arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.ToStringInfo()}");
        }

        public void ConsumeBinaryRequest(HttpSource arg1, HttpBinaryMapped arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.Fields}");
        }

        public void consumeBinaryResponse(HttpSource arg1, HttpBinaryMapped arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.Fields}");
        }
        public static HttpConsumer GetOrCreateShared()
        {
            if (Shared == null)
            {
                lock (lockShared)
                {
                    if (Shared is null)
                    {
                        Shared = new HttpRecoreder();
                    }
                }
            }
            return Shared;
        }
        public override void PushRequestHttp20(HttpSource dataSource, ReadOnlyMemory<byte> memClient)
        {
            base.PushRequestHttp20(dataSource, memClient);
        }
        public override void PushResponseHttp20(HttpSource dataSource, ReadOnlyMemory<byte> memClient)
        {
            base.PushResponseHttp20(dataSource, memClient);
        }
    }
}