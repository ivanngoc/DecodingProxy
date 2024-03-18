using Google.Protobuf;
using Grpc.Net.Client;
using IziHardGames.Pools.Abstractions.NetStd21;
using static IziHardGames.Libs.gRPC.Hubs.GrpcHub;
using BinaryMessage = IziHardGames.Libs.gRPC.Hubs.BinaryMessage;

namespace IziHardGames.Libs.gRPC.InterprocessCommunication
{
    public class UniClient : IDisposable
    {
        private string name;
        private int counter;
        private GrpcHubClient client;
        private GrpcChannel channel;

        public async ValueTask<RequestResult> SendRequest(int action, ReadOnlyMemory<byte> value)
        {
            throw new System.NotImplementedException();
        }
        public async ValueTask<T> RequestObjectAsync<T>(int action)
        {
            throw new System.NotImplementedException();
        }
        public async ValueTask SendObject(int action, ReadOnlyMemory<byte> value)
        {
            var msg = PoolObjectsConcurent<BinaryMessage>.Shared.Rent();
            msg.Id = ++counter;
            msg.Type = action;
            msg.Length = value.Length;
            msg.Bytes = ByteString.CopyFrom(value.Span);
            var result = await client.ExchangeBinaryAsync(msg).ConfigureAwait(false);
            PoolObjectsConcurent<BinaryMessage>.Shared.Return(msg);
        }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        public static async Task Test()
        {
            string address = "http://localhost:5104";

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
            var channel = Grpc.Net.Client.GrpcChannel.ForAddress(address, opt);
            var service = new IziHardGames.Libs.gRPC.Hubs.GrpcHub.GrpcHubClient(channel);
            int counter = 0;

            BinaryMessage binaryMessage = new BinaryMessage()
            {
                Id = counter++,
                Type = 2,
                Length = 10,
                Bytes = Google.Protobuf.ByteString.CopyFrom(new byte[10]),
            };
            var result = await service.ExchangeBinaryAsync(binaryMessage).ConfigureAwait(false);
        }

        public readonly struct RequestResult : IDisposable
        {
            public RequestResult(BinaryMessage message)
            {
                //this.pool = message;
            }
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

    }
}
