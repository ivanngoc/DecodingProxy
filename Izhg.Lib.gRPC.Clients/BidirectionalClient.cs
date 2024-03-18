using System.Text;
using System.Text.Json.Nodes;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using IziHardGames.Libs.gRPC.Clients.Attributes;
using IziHardGames.Libs.gRPC.Clients.Contracts;
using IziHardGames.Pools.Abstractions.NetStd21;
using static IziHardGames.Libs.gRPC.Hubs.GrpcHub;
using BinaryMessage = IziHardGames.Libs.gRPC.Hubs.BinaryMessage;
using Func = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<System.ReadOnlyMemory<byte>>>;


namespace IziHardGames.Libs.gRPC.InterprocessCommunication
{

    /// <summary>
    /// 
    /// </summary>
    public class BidirectionalClient : IDisposable, IGrpcClient
    {
        SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        private static int counter;

        private string name;

        private GrpcHubClient? client;
        private AsyncDuplexStreamingCall<BinaryMessage, BinaryMessage> streams;
        private IClientStreamWriter<BinaryMessage> requests;
        private IAsyncStreamReader<BinaryMessage> responses;
        private GrpcChannel? channel;

        private int counterSending = 0;
        private int clientId;
        public Func[] funcs;

        public BidirectionalClient(string name)
        {
            this.name = name;
            Interlocked.Increment(ref counter);
        }


        [Bidirectional]
        public async Task ConnectForServerRequestsAsync(string address, CancellationToken ct = default)
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
            this.client = new IziHardGames.Libs.gRPC.Hubs.GrpcHub.GrpcHubClient(channel);//.Retry(10);

            //this.streams = client.BidirectExchange();
            //this.requests = streams.RequestStream;
            //this.responses = streams.ResponseStream;
            //await requests.WriteAsync(new BinaryMessage());

            this.streams = client.ServerRequests(null, null, ct);
            this.requests = streams.RequestStream;
            this.responses = streams.ResponseStream;
            counterSending++;

            JsonObject jsonObject = new JsonObject()
            {
                [ConstantsGrpc.Properties.MESSAGE_TYPE] = ConstantsGrpc.Message.TYPE_AUTH,
            };
            byte[] bytes = Encoding.UTF8.GetBytes(jsonObject.ToJsonString());
            BinaryMessage binaryMessage = PoolObjectsConcurent<BinaryMessage>.Shared.Rent();
            binaryMessage.Id = counterSending;
            binaryMessage.Type = ConstantsGrpc.Message.TYPE_AUTH;
            binaryMessage.Length = bytes.Length;
            binaryMessage.Bytes = ByteString.CopyFrom(bytes);
            binaryMessage.ClientId = clientId;

            await requests.WriteAsync(binaryMessage, ct).ConfigureAwait(false);
            Console.WriteLine($"Auth request sended");

            var isNext = await responses.MoveNext(ct).ConfigureAwait(false);
            if (isNext)
            {
                var response = responses.Current;
                var respJson = JsonObject.Parse(response.Bytes.Span)!;
                clientId = (int)respJson[ConstantsGrpc.Properties.CLIENT_ID]!;
                Console.WriteLine($"Auth passed. ClientID:{clientId}");
            }
            PoolObjectsConcurent<BinaryMessage>.Shared.Return(binaryMessage);
        }

        [Bidirectional]
        public void Connect(string address, CancellationToken ct = default)
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
            this.client = new IziHardGames.Libs.gRPC.Hubs.GrpcHub.GrpcHubClient(channel);//.Retry(10);

            this.streams = client.BidirectExchange(null, null, ct);
            this.requests = streams.RequestStream;
            this.responses = streams.ResponseStream;
        }

        public async Task RecieveAndHandleAsync(CancellationToken ct = default)
        {
            BinaryMessage response = PoolObjectsConcurent<BinaryMessage>.Shared.Rent();
            var isNext = await responses.MoveNext(ct).ConfigureAwait(false);
            if (isNext)
            {
                var request = responses.Current;
                var bytes = request.Bytes;
                Console.WriteLine($"RecieveAndHandleAsync:[{Encoding.UTF8.GetString(bytes.Span)}]");
                var action = request.Type;
                var result = await funcs[action].Invoke(request.Bytes.Memory).ConfigureAwait(false);
                response.Id = request.Id;
                response.Type = request.Type;
                response.Length = result.Length;
                response.Bytes = ByteString.CopyFrom(result.Span);
                response.ClientId = clientId;
                await requests.WriteAsync(response).ConfigureAwait(false);
            }
            PoolObjectsConcurent<BinaryMessage>.Shared.Return(response);
        }
        public async ValueTask<BinaryMessage> RecieveAsync(CancellationToken ct = default)
        {
            var result = await responses.MoveNext(ct).ConfigureAwait(false);
            if (result)
            {
                return responses.Current;
            }
            throw new System.NotImplementedException();
        }

        public void Dispose()
        {
            streams.Dispose();
            clientId = default;
        }

        public async Task SendObjectAsync(int action, byte[] bytes)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                BinaryMessage msg = PoolObjectsConcurent<BinaryMessage>.Shared.Rent();
                counterSending++;

                msg.Id = counterSending;
                msg.Type = action;
                msg.Length = bytes.Length;
                msg.Bytes = ByteString.CopyFrom(bytes);
                msg.ClientId = clientId;
                await requests.WriteAsync(msg).ConfigureAwait(false);
                PoolObjectsConcurent<BinaryMessage>.Shared.Return(msg);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                semaphoreSlim.Release();
            }
        }

        public void SendObjectAsync(object aCTION_MARK_AS_INFO_PROVIDER, byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public void SetFuncs(Func[] funcs)
        {
            this.funcs = funcs;
        }
    }
}
