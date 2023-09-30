using Grpc.Core;
using IziHardGames.Libs.gRPC.Examples;

namespace IziHardGames.Libs.gRPC.Example
{
    public class EchoService : IziHardGames.Libs.gRPC.Examples.EchoService.EchoServiceBase
    {
        public override Task<EchoResponse> ClientStreamingRpc(IAsyncStreamReader<EchoRequest> requestStream, ServerCallContext context)
        {
            throw new System.NotImplementedException();
        }
        public override Task ServerStreamingRpc(EchoRequest request, IServerStreamWriter<EchoResponse> responseStream, ServerCallContext context)
        {
            throw new System.NotImplementedException();
        }
        public override Task<EchoResponse> UnaryRpc(EchoRequest request, ServerCallContext context)
        {
            throw new System.NotImplementedException();
        }
        public override async Task BidirectionalStreamingRpc(IAsyncStreamReader<EchoRequest> requestStream, IServerStreamWriter<EchoResponse> responseStream, ServerCallContext context)
        {
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var result = await requestStream.MoveNext();
                if (result)
                {
                    string value = requestStream.Current.ValueString;
                    await responseStream.WriteAsync(new EchoResponse() { ValueString = $"Echo\t{result}" });
                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}]\tServer recived:\t{value}");
                }
            }
        }
    }
}
