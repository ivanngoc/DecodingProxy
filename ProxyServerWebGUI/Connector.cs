using Grpc.Net.Client;
using IziHardGames.Proxy.gRPC;
using IziHardGames.Proxy.Tcp.Tls;
using static IziHardGames.Proxy.gRPC.ProxyPublisher;

namespace IziHardGames.Proxy.WebGUI
{
    public class Connector
    {
        private ProxyPublisherClient client;
        public void Connect()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5002");
            client = new IziHardGames.Proxy.gRPC.ProxyPublisher.ProxyPublisherClient(channel);
        }

        public IList<DataConnection> GetConnections()
        {
            var reply = client.GetHubs(new gRPC.Request() { Name = "this is dump name" });
            return reply.Connection;
        }
    }
}