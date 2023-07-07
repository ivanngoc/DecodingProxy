using Grpc.Core;
using IziHardGames.Proxy.gRPC;
using static IziHardGames.Proxy.gRPC.ProxyPublisher;

namespace IziHardGames.Proxy
{
    public class ProxyPublisherService : ProxyPublisherBase
    {
        private readonly ILogger logger;
        public ProxyPublisherService(ILogger<ProxyPublisherService> logger)
        {
            this.logger = logger;
        }

        public override Task<Reply> GetHubs(Request request, ServerCallContext context)
        {
            return Task.FromResult<Reply>(new Reply() { Message = $"{DateTime.Now} this is message with id = 1000" });
        }
    }
}