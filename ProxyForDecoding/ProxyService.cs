// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Http;
using IziHardGames.Proxy.Recoreder;
using System.Text;
using HttpRequest = HttpDecodingProxy.ForHttp.HttpRequest;

namespace IziHardGames.Proxy
{
    public class ProxyService : IHostedService
    {
        private readonly ILogger logger;
        private DecodingProxyServerAPI grpcServer;
        private HttpSpyProxy httpDecodingProxyServer;
        private readonly HttpRecoreder httpRecoreder;
        private readonly ConsumingProvider consumingProvider;

        public ProxyService(ILogger<ProxyService> logger, DecodingProxyServerAPI grpcServer, HttpSpyProxy httpDecodingProxyServer, HttpRecoreder httpRecoreder)
        {
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            Console.InputEncoding = System.Text.Encoding.Unicode;

            // Console.OutputEncoding = Encoding.UTF8;
            HttpFieldsV11.loggerShared = logger;

            this.logger = logger;
            this.grpcServer = grpcServer;
            this.httpDecodingProxyServer = httpDecodingProxyServer;
            this.httpRecoreder = httpRecoreder;
            Logger.logger = logger;
            logger.Log(LogLevel.Warning, "Created proxy sevice");
            logger.Log(LogLevel.Information, "Log Information");

            consumingProvider = new ConsumingProvider()
            {
                consumersRequest = new IBlockConsumer[] { httpRecoreder.requestRecorer },
                consumersResponse = new IBlockConsumer[] { httpRecoreder.responseRecorder },
                consumeRequest = httpRecoreder.RecieveRequest,
                consumeResponse = httpRecoreder.RecieveResponse,
                consumeRequestMsg = httpRecoreder.RecieveRequestMsg,
                consumeResponseMsg = httpRecoreder.RecieveResponseMsg,
                consumeBinaryRequest = httpRecoreder.ConsumeBinaryRequest,
                consumeBinaryResponse = httpRecoreder.consumeBinaryResponse,
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Log(LogLevel.Warning, "Created proxy sevice");
            await Task.Run(async () => await httpDecodingProxyServer.Run(logger, consumingProvider), cancellationToken).ConfigureAwait(false);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            httpDecodingProxyServer.Stop();
        }
    }
}