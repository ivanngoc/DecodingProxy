using IziHardGames.Proxy.Sniffing.ForHttp;
using System.IO;
using System.Net.Security;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Streaming
{
    public class StreamToStreamFactory
    {
        public static async Task RunHttpStreamToStreamWithPipeline(SslStream from, SslStream to)
        {
            PipedHttpStream request = new PipedHttpStream();
            PipedHttpStream response = new PipedHttpStream();

            while (true)
            {
                var t1 = request.CopyOnce(response);
                var t2 = response.CopyOnce(request);

                Task.WaitAll(t1, t2);

                //request.TryReadHttpMsg();
                //response.TryReadHttpMsg();
            }

            throw new System.NotImplementedException();
        }

        public static Task RunStreamToStreamWithPipeline(Stream from, Stream to)
        {
            throw new System.NotImplementedException();
        }

        public async static Task Test()
        {

        }
    }
}