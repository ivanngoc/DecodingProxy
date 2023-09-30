using System.Threading;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.ForHttp;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Http;
using IziHardGames.Proxy.Recoreder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace IziHardGames.Proxy
{
    /// <summary>
    /// Управляет запусков и остановкой всех прокси сервисов
    /// </summary>
    public class ProxyService : IHostedService
    {
        private readonly ILogger logger;
        private HttpSpyProxy proxyServer;
        private readonly HttpRecoreder httpRecoreder;
        private readonly ConsumingProvider consumingProvider;


        public ProxyService(ILogger<ProxyService> logger, HttpSpyProxy proxy, HttpRecoreder httpRecoreder)
        {
            // Console.OutputEncoding = Encoding.UTF8;
            HttpFieldsV11.loggerShared = logger;

            this.logger = logger;
            this.proxyServer = proxy;
            this.httpRecoreder = httpRecoreder;
            Logger.logger = logger;
            logger.Log(LogLevel.Information, $"Created {typeof(ProxyService).FullName}");
            httpRecoreder.SetManager(new ManagerForHttpSessionDefault());

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
            var task = Task.Run(async () => await proxyServer.Run(consumingProvider), cancellationToken).ConfigureAwait(false);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            proxyServer.Break();
            return Task.CompletedTask;
        }
    }
}