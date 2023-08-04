// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Http;
using IziHardGames.Proxy.Recoreder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpRequest = HttpDecodingProxy.ForHttp.HttpRequest;

namespace IziHardGames.Proxy
{
    public class ProxyService : IHostedService
    {
        private readonly ILogger logger;
        private GrpcServiceServer grpcServer;
        private HttpSpyProxy httpDecodingProxyServer;
        private readonly HttpRecoreder httpRecoreder;
        private readonly ConsumingProvider consumingProvider;

        public ProxyService(ILogger<ProxyService> logger, GrpcServiceServer grpcServer, HttpSpyProxy httpDecodingProxyServer, HttpRecoreder httpRecoreder)
        {
            // Console.OutputEncoding = Encoding.UTF8;
            HttpFieldsV11.loggerShared = logger;

            this.logger = logger;
            this.grpcServer = grpcServer;
            this.httpDecodingProxyServer = httpDecodingProxyServer;
            this.httpRecoreder = httpRecoreder;
            Logger.logger = logger;
            logger.Log(LogLevel.Information, $"Created {typeof(ProxyService).FullName}");

            consumingProvider = new ConsumingProvider()
            {
                consumeRequest = httpRecoreder.RecieveRequest,
                consumeResponse = httpRecoreder.RecieveResponse,
                consumeRequestMsg = httpRecoreder.RecieveRequestMsg,
                consumeResponseMsg = httpRecoreder.RecieveResponseMsg,
                consumeBinaryRequest = httpRecoreder.ConsumeBinaryRequest,
                consumeBinaryResponse = httpRecoreder.consumeBinaryResponse,
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // do not await. it's caused blocking (for example gRPC might not start yet)
            var task = Task.Run(async () => await httpDecodingProxyServer.Run(consumingProvider), cancellationToken).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            httpDecodingProxyServer.Break();
            return Task.CompletedTask;
        }
    }
}