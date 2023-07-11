// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using Grpc.Core;
using IziHardGames.Proxy.gRPC;
using IziHardGames.Proxy.Http;
using static IziHardGames.Proxy.gRPC.ProxyPublisher;

namespace IziHardGames.Proxy
{

    /// <summary>
    /// Provides Methods to acquire datas by UI apps
    /// </summary>
    public class DecodingProxyServerAPI : ProxyPublisherBase
    {
        private HttpSpyProxy httpDecodingProxyServer;
        public DecodingProxyServerAPI(HttpSpyProxy httpDecodingProxyServer)
        {
            this.httpDecodingProxyServer = httpDecodingProxyServer;
        }

        public override Task<Reply> GetHubs(Request request, ServerCallContext context)
        {
            var reply = new Reply();
            reply.Connection.AddRange(httpDecodingProxyServer.managerForConnectionsToDomain.items.Values.Select(x => new DataConnection()
            {
                Domain = x.Key,
                Count = x.Count,
            }));
            return Task.FromResult<Reply>(reply);
        }
    }
}