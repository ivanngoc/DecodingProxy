using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Monitoring;
using Data = IziHardGames.Proxy.gRPC.ProtobufDataConnection;

namespace IziHardGames.Proxy.Http
{
    public class MonitorForConnections : IGrpcNotifier<IConnectionData>
    {
        private GrpcProxyPublisherService grpc;
        public MonitorForConnections(GrpcProxyPublisherService grpc)
        {
            this.grpc = grpc;
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