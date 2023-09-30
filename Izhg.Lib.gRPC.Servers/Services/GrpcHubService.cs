using System.Collections.Concurrent;
using System.Text;
using System.Text.Json.Nodes;
using Google.Protobuf;
using Grpc.Core;
using IziHardGames.Libs.gRPC.Hubs;
using IziHardGames.Libs.gRPC.InterprocessCommunication;
using IziHardGames.Libs.NonEngine.Memory;
using Func = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<System.ReadOnlyMemory<byte>>>;

namespace IziHardGames.Libs.gRPC.Services
{
    internal class Client : IDisposable
    {
        public readonly int id;
        private int version;
        internal IAsyncStreamReader<BinaryMessage> requestStream;
        internal IServerStreamWriter<BinaryMessage> responseStream;

        public Client()
        {
            id = GetHashCode();
        }

        public void Dispose()
        {
            version++;
            requestStream = default;
            responseStream = default;
        }
    }
    public class GrpcHubService : GrpcHub.GrpcHubBase
    {
        private readonly ILogger<GrpcHubService> _logger;
        private Func[] funcs;
        private int counter;
        public static readonly VoidReply empty = new VoidReply();
        public static int countCreated;
        private readonly ConcurrentDictionary<int, Client> clients = new ConcurrentDictionary<int, Client>();

        public GrpcHubService(ILogger<GrpcHubService> logger)
        {
            Interlocked.Increment(ref counter);
            _logger = logger;
        }

        public override async Task BidirectExchange(IAsyncStreamReader<BinaryMessage> requestStream, IServerStreamWriter<BinaryMessage> responseStream, ServerCallContext context)
        {
            BinaryMessage response = PoolObjectsConcurent<BinaryMessage>.Shared.Rent();
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var isMoveNext = await requestStream.MoveNext().ConfigureAwait(false);
                if (isMoveNext)
                {
                    var request = requestStream.Current;
                    var result = await funcs[request.Type].Invoke(request.Bytes.Memory).ConfigureAwait(false);
                    response.Id = request.Id;
                    response.Type = request.Type;
                    response.Length = result.Length;
                    response.Bytes = ByteString.CopyFrom(result.Span);
                    await responseStream.WriteAsync(response).ConfigureAwait(false);
                }
            }
            PoolObjectsConcurent<BinaryMessage>.Shared.Return(response);
        }

        public override async Task ServerRequests(IAsyncStreamReader<BinaryMessage> requestStream, IServerStreamWriter<BinaryMessage> responseStream, ServerCallContext context)
        {
            Console.WriteLine($"Called [{nameof(ServerRequests)}]");
            Client client = PoolObjectsConcurent<Client>.Shared.Rent();
            clients.TryAdd(client.id, client);
            client.requestStream = requestStream;
            client.responseStream = responseStream;

            BinaryMessage response = PoolObjectsConcurent<BinaryMessage>.Shared.Rent();
            while (!context.CancellationToken.IsCancellationRequested)
            {
                var isMoveNext = await requestStream.MoveNext().ConfigureAwait(false);
                if (isMoveNext)
                {
                    var request = requestStream.Current;
                    Console.WriteLine($"[{nameof(ServerRequests)}] Recived: [{Encoding.UTF8.GetString(request.Bytes.Span)}]");
                    response.Id = request.Id;
                    var json = JsonNode.Parse(request.Bytes.Span)!.AsObject();
                    var msgType = (int)json[ConstantsGrpc.Properties.MESSAGE_TYPE]!;
                    if (msgType == ConstantsGrpc.Message.TYPE_AUTH)
                    {
                        JsonObject jsonResponse = new JsonObject()
                        {
                            [ConstantsGrpc.Properties.CLIENT_ID] = client.id,
                        };
                        var bytes = Encoding.UTF8.GetBytes(jsonResponse.ToJsonString());
                        response.Type = request.Type;
                        response.Bytes = ByteString.CopyFrom(bytes);
                        response.Length = bytes.Length;
                        await responseStream.WriteAsync(response).ConfigureAwait(false);
                    }
                    else if (msgType == ConstantsGrpc.Message.TYPE_DATA_FOR_SERVER)
                    {
                        var result = await funcs[request.Type].Invoke(request.Bytes.Memory).ConfigureAwait(false);
                        response.Type = request.Type;
                        if (result.Length > 0)
                        {
                            response.Length = result.Length;
                            response.Bytes = ByteString.CopyFrom(result.Span);
                        }
                        else
                        {
                            response.Length = 0;
                            response.Bytes = ByteString.Empty;
                        }
                        await responseStream.WriteAsync(response).ConfigureAwait(false);
                    }
                    else
                    {
                        response.Type = ConstantsGrpc.Headers.TYPE_ERROR;
                        response.Bytes = ByteString.CopyFrom($"Authentication failed", Encoding.UTF8);
                        response.Length = response.Bytes.Length;
                        await responseStream.WriteAsync(response).ConfigureAwait(false);
                        break;
                    }
                }
            }
            PoolObjectsConcurent<BinaryMessage>.Shared.Return(response);
            if (!clients.TryRemove(client.id, out client)) throw new InvalidOperationException();
            client.Dispose();
            PoolObjectsConcurent<Client>.Shared.Return(client);
        }

        public override async Task<BinaryMessage> ExchangeBinary(BinaryMessage request, ServerCallContext context)
        {
            var result = await funcs[request.Type].Invoke(request.Bytes.Memory).ConfigureAwait(false);
            counter++;
            request.Id = counter;
            request.Length = result.Length;
            if (result.Length > 0)
            {
                request.Bytes = ByteString.CopyFrom(result.Span);
            }
            else
            {
                request.Bytes = ByteString.Empty;
            }
            return request;
        }

        public override Task<Reply> SendObject(Request request, ServerCallContext context)
        {
            return base.SendObject(request, context);
        }

        public async Task SendToSubscribersAsync(int action, byte[] bytes)
        {
            if (clients.IsEmpty) return;
            var msg = PoolObjectsConcurent<BinaryMessage>.Shared.Rent();
            foreach (var pair in clients)
            {
                var client = pair.Value;
                msg.Id = 0;
                msg.Type = action;
                msg.Length = bytes.Length;
                msg.Bytes = ByteString.CopyFrom(bytes);
                msg.ClientId = client.id;
                await client.responseStream.WriteAsync(msg).ConfigureAwait(false);
            }
            PoolObjectsConcurent<BinaryMessage>.Shared.Return(msg);
        }

        public override async Task<VoidReply> PushBinary(BinaryMessage request, ServerCallContext context)
        {
            var result = funcs[request.Type].Invoke(request.Bytes.Memory);
            return empty;
        }
        public void SetHandlers(Func[] funcs)
        {
            this.funcs = funcs;
        }

        public async Task SendRequest()
        {
            throw new System.NotImplementedException();
        }


    }
}