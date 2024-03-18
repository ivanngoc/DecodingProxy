using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Proxy.gRPC;
using Microsoft.Extensions.Logging;
using DataConnection = IziHardGames.Proxy.gRPC.ProtobufDataConnection;
using Reply = IziHardGames.Proxy.gRPC.ProtobufReply;
using Request = IziHardGames.Proxy.gRPC.ProtobufRequest;

namespace IziHardGames.Proxy
{
    public class GrpcProxyPublisherService : IziHardGames.Proxy.gRPC.ProxyPublisher.ProxyPublisherBase
    {
        public readonly ConcurrentDictionary<uint, GrpcConnection> keyValues = new ConcurrentDictionary<uint, GrpcConnection>();
        private uint idCounter;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private readonly ILogger<GrpcProxyPublisherService> _logger;

        public static GrpcProxyPublisherService singleton;
        private readonly static object lockSingleton = new object();
        public GrpcProxyPublisherService(ILogger<GrpcProxyPublisherService> logger)
        {
            this._logger = logger;
            // check that gRPC framework doesn't create duplicate when we use services.AddSingleton<ThisClassName>()
            //Console.WriteLine($"Created GrpcProxyPublisherService.{Environment.NewLine}{Environment.StackTrace}");
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