using IziHardGames.Libs.gRPC.Services;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Monitoring;
using Func = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<System.ReadOnlyMemory<byte>>>;

namespace IziHardGames.Proxy.Http
{
    public class MonitorForConnectionsGrpc : IGrpcNotifier<IConnectionData>
    {
        private GrpcProxyPublisherService grpc;
        public MonitorForConnectionsGrpc(GrpcProxyPublisherService grpc, GrpcHubService grpcHubService)
        {
            this.grpc = grpc;
            Func[] funcs = new Func[]
            {
                // request 
            };
            grpcHubService.SetHandlers(funcs);
        }
        public void OnAdd(IConnectionData item)
        {
            item.Action = ConstantsMonitoring.ACTION_ADD;
            grpc.NotifyAll(item);
        }

        public void OnUpdate(IConnectionData item)
        {
            item.Action = ConstantsMonitoring.ACTION_UPDATE;
            grpc.NotifyAll(item);
        }

        public void OnRemove(IConnectionData item)
        {
            item.Action = ConstantsMonitoring.ACTION_REMOVE;
            grpc.NotifyAll(item);
        }
    }
}