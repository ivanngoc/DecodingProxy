using IziHardGames.Proxy.Consuming;
using System;
using System.IO;
using IziHardGames.Proxy;

namespace IziHardGames.Libs.ForHttp
{
    public class HttpSessionDefault : HttpSession, IDisposable
    {
        public int id;
        public int version;
        public string host;
        public int port;
        public EHttpConnectionFlags flagsAgent;
        public EHttpConnectionFlags flagsOrigin;

        public readonly MemoryStream streamResponse = new MemoryStream((1 << 20) * 32);
        public readonly MemoryStream streamRequest = new MemoryStream((1 << 20) * 32);
        public override void AddResponse20(ReadOnlyMemory<byte> mem)
        {
            lock (streamResponse)
            {
                streamResponse.Write(mem.Span);
            }
        }
        public override void AddRequest20(ReadOnlyMemory<byte> mem)
        {
            lock (streamRequest)
            {
                streamRequest.Write(mem.Span);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void AddResponse11(ReadOnlyMemory<byte> mem)
        {
            lock (streamResponse)
            {
                streamResponse.Write(mem.Span);
            }
        }

        public override void AddRequest11(ReadOnlyMemory<byte> mem)
        {
            lock (streamRequest)
            {
                streamRequest.Write(mem.Span);
            }
        }
        public override void CheckClose(HttpSource dataSource)
        {

        }
    }
}
