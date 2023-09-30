using Grpc.Core;
using Grpc.Net.Client;
using IziHardGames.Libs.gRPC.Examples;
using static IziHardGames.Libs.gRPC.Examples.EchoService;

namespace IziHardGames.Libs.gRPC.InterprocessCommunication
{
    public class EchoClient : IDisposable
    {
        private EchoServiceClient? client;
        private GrpcChannel? channel;
        private AsyncDuplexStreamingCall<EchoRequest, EchoResponse> streams;

        internal static async Task Test()
        {
            EchoClient client = new EchoClient();
            client.Connect("http://localhost:5253");
            int counter = default;

            var t1 = Task.Run(async () =>
               {
                   while (true)
                   {
                       var stream = client.streams.ResponseStream;
                       var result = await stream.MoveNext();
                       if (result)
                       {
                           var response = stream.Current;
                           Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]\tClient recived:\t{response.ValueString}");
                       }
                   }
               });

            while (true)
            {
                await Task.Delay(1000);
                await client.SendMessageAsync($"this is message {counter}");
            }
            await t1.ConfigureAwait(false);
        }

        private async Task SendMessageAsync(string v)
        {
            await streams.RequestStream.WriteAsync(new EchoRequest() { ValueString = v }).ConfigureAwait(false);
        }

        private void SendMessage(string v)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Connect(string address)
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true
            };

            GrpcChannelOptions opt = new GrpcChannelOptions()
            {
                HttpHandler = handler,
            };


            this.channel = Grpc.Net.Client.GrpcChannel.ForAddress(address, opt);
            this.client = new IziHardGames.Libs.gRPC.Examples.EchoService.EchoServiceClient(channel);
            this.streams = client.BidirectionalStreamingRpc(null, null, default);
            this.streams.Dispose();
        }
    }
}