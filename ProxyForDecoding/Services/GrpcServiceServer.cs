using Grpc.Core;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.gRPC;
using IziHardGames.Proxy.Http;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DataConnection = IziHardGames.Proxy.gRPC.ProtobufDataConnection;
using Reply = IziHardGames.Proxy.gRPC.ProtobufReply;
using Request = IziHardGames.Proxy.gRPC.ProtobufRequest;

namespace IziHardGames.Proxy
{

    /// <summary>
    /// Provides Methods to acquire datas by UI apps
    /// </summary>
    public class GrpcServiceServer
    {

    }

    public class GrpcProxyPublisherService : IziHardGames.Proxy.gRPC.ProxyPublisher.ProxyPublisherBase
    {
        public readonly ConcurrentDictionary<uint, GrpcConnection> keyValues = new ConcurrentDictionary<uint, GrpcConnection>();
        private uint idCounter;
        private CancellationTokenSource cts = new CancellationTokenSource();
        //private readonly ILogger<GrpcService> _logger;

        public static GrpcProxyPublisherService singleton;
        private readonly static object lockSingleton = new object();
        public GrpcProxyPublisherService()
        {
            // check that gRPC framework doesn't create duplicate when we use services.AddSingleton<ThisClassName>()
            Console.WriteLine($"Created GrpcProxyPublisherService.{Environment.NewLine}{Environment.StackTrace}");

            if (singleton == null)
            {
                lock (lockSingleton)
                {
                    if (singleton == null)
                    {
                        singleton = this;
                    }
                    else
                    {
                        throw new System.NotImplementedException();
                    }
                }
            }
        }

        public async override Task<Reply> GetHubs(Request request, ServerCallContext context)
        {
            //var reply = new Reply();
            //reply.Connection.AddRange(httpDecodingProxyServer.managerForConnectionsToDomain.items.Values.Select(x => new DataConnection()
            //{
            //    Domain = x.Key,
            //    Count = x.Count,
            //}));
            //return Task.FromResult<Reply>(reply);
            return await Task.FromResult<Reply>(new Reply() { Message = $"{DateTime.Now} this is message with id = 1000" });
        }

        public async override Task<ProtobufEmpty> Push(DataConnection request, ServerCallContext context)
        {
            return await base.Push(request, context);
        }

        public async override Task MaintainConnection(IAsyncStreamReader<Request> reader, IServerStreamWriter<Reply> writer, ServerCallContext context)
        {
            var id = Interlocked.Increment(ref idCounter);
            GrpcConnection grpcConnections = PoolObjectsConcurent<GrpcConnection>.Shared.Rent();
            grpcConnections.id = id;
            grpcConnections.pool = PoolObjectsConcurent<GrpcConnection>.Shared;
            grpcConnections.reader = reader;
            grpcConnections.writer = writer;
            grpcConnections.dic = keyValues;
            grpcConnections.context = context;
            Task t1 = Task.Run(async () => await grpcConnections.RunAsServerNotifier(cts.Token).ConfigureAwait(false));
            grpcConnections.task = t1;
            if (!keyValues.TryAdd(id, grpcConnections))
            {
                throw new ArgumentOutOfRangeException($"ConcurrentDictionary already got id:{id}");
            }
            await t1;
        }

        public void NotifyAll(IConnectionData data)
        {
            foreach (var item in keyValues.Values)
            {
                item.Put(data);
            }
        }
    }
}