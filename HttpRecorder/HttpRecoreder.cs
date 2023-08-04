using HttpDecodingProxy.ForHttp;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Recoreder.MemoryBased;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using HttpResult = HttpDecodingProxy.ForHttp.HttpObject;

namespace IziHardGames.Proxy.Recoreder
{
    public class HttpRecoreder
    {
        public readonly RequestMemoryRecorder requestMemoryRecorder = new RequestMemoryRecorder();
        public readonly ResponseMemoryRecorder responseMemoryRecorder = new ResponseMemoryRecorder();
        public const bool isLog = false;

        public HttpRecoreder()
        {
        }

        private void RecieveRequest(DataSource source, HttpResult item)
        {
            if (isLog) Console.WriteLine(item.ToStringInfo());
        }
        private void RecieveResponse(DataSource source, HttpResult item)
        {
            if (isLog) Console.WriteLine(item.ToStringInfo());
        }

        public async Task RecieveResponse(DataSource dataSource, ReadOnlySequence<byte> data)
        {
            requestMemoryRecorder.Push(dataSource, data);
        }
        public async Task RecieveRequest(DataSource dataSource, ReadOnlySequence<byte> data)
        {
            requestMemoryRecorder.Push(dataSource, data);
        }

        public void RecieveRequestMsg(DataSource arg1, HttpResult arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.ToStringInfo()}");
        }
        public void RecieveResponseMsg(DataSource arg1, HttpResult arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.ToStringInfo()}");
        }

        public void ConsumeBinaryRequest(DataSource arg1, HttpBinaryMapped arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.Fields}");
        }

        public void consumeBinaryResponse(DataSource arg1, HttpBinaryMapped arg2)
        {
            if (isLog) Console.WriteLine($"{arg1.ToStringInfo()}{Environment.NewLine}{arg2.Fields}");
        }
    }
}