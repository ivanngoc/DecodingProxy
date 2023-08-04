using Grpc.Core;
using Grpc.Net.Client;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.gRPC;
using IziHardGames.Proxy.Tcp.Tls;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static IziHardGames.Proxy.gRPC.ProxyPublisher;
using DataConnection = IziHardGames.Proxy.gRPC.ProtobufDataConnection;
using Reply = IziHardGames.Proxy.gRPC.ProtobufReply;
using Request = IziHardGames.Proxy.gRPC.ProtobufRequest;

namespace IziHardGames.Proxy.WebGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class GrpcConnector
    {
        /// <summary>
        /// Long lived connection for recieving push notifications
        /// </summary>
        private ProxyPublisherClient? clientForNotifications;
        //private string address = "http://localhost:5002";
        private string address = "http://localhost:5104";
        private GrpcChannel channelForNotifications;
        private readonly ProxyChangeReflector reflector;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private ILogger<GrpcConnector> logger;
        private Task task;
        public GrpcConnector(ILogger<GrpcConnector> logger, ProxyChangeReflector reflector)
        {
            this.logger = logger;
            this.reflector = reflector;
            ConnectLongLived();
        }
        public ProxyPublisherClient Connect()
        {
            channelForNotifications = GrpcChannel.ForAddress(address);
            clientForNotifications = new IziHardGames.Proxy.gRPC.ProxyPublisher.ProxyPublisherClient(channelForNotifications);
            return clientForNotifications;
        }
        public void ConnectLongLived()
        {
            logger.LogInformation($"{typeof(GrpcConnector).FullName} gRPC is callling {nameof(ConnectLongLived)}");

            var token = cts.Token;

            this.task = Task.Factory.StartNew(async () =>
            {
                var client = Connect();
                var stream = client.MaintainConnection();

                var reader = stream.ResponseStream;
                var writer = stream.RequestStream;

                while (!token.IsCancellationRequested)
                {
                    var isGot = await reader.MoveNext(token);
                    if (isGot)
                    {
                        var reply = reader.Current;
                        var pool = PoolObjectsConcurent<DefaultConnectionData>.Shared;
                        DefaultConnectionData dcd = pool.Rent();
                        dcd.BindToPool(pool);
                        var data = reply.Connection;
                        dcd.Id = data.Id;
                        dcd.Host = data.Host;
                        dcd.Port = data.Port;
                        dcd.Status = data.Status;
                        dcd.Action = data.Action;
                        reflector.Recieve(dcd);
                    }
                }
                stream.Dispose();
            }, token);
        }

        public async Task<IList<DataConnection>> RequestConnectionsAsync()
        {
            logger.LogInformation($"{typeof(GrpcConnector).FullName} gRPC is callling {nameof(RequestConnectionsAsync)}");
            try
            {
                using (var channel = GrpcChannel.ForAddress(address))
                {
                    var client = new IziHardGames.Proxy.gRPC.ProxyPublisher.ProxyPublisherClient(channel);
                    var reply = await client!.GetHubsAsync(new Request() { Name = "this is dump name" }).ConfigureAwait(false);
                    return reply.Connections;
                }
            }
            catch (SocketException ex)
            {   // connection error. check gRPC server is running
                throw ex;
            }
        }
        public async Task<IEnumerable<IDomainData>> RequestDomainsAsync()
        {
            await Task.Delay(1);
            return Enumerable.Empty<IDomainData>();
        }

        public void RecieveConnection(IConnection data)
        {
            logger.LogInformation($"{typeof(GrpcConnector).FullName} gRPC is callling {nameof(RecieveConnection)}");
            var temp = PoolObjectsConcurent<DataConnection>.Shared.Rent();
            temp.Host = data.Host;
            temp.Port = data.Port;
            clientForNotifications!.Push(temp);
        }
    }
}