using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Sniffing.ForHttp;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.Http
{
    public class ProxyModeHttp : IDisposable
    {
        private ILogger logger;
        private byte[] buffer;
        private byte[] bufferToOrigin;
        private HttpObjectStream httpStreamRequest;
        private HttpObjectStream httpStreamResponse;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private TcpClient cto;
        private bool isConnectionClose;
        private NetworkStream nsToAgent;
        private NetworkStream nsToOrigin;
        private int readedFromAgent;

        public async Task Run(ILogger logger, NetworkStream nsToAgent)
        {
            logger.LogInformation($"начал обрабатывать http соединение");

            httpStreamRequest = PoolObjectsConcurent<HttpObjectStream>.Shared.Rent();
            httpStreamResponse = PoolObjectsConcurent<HttpObjectStream>.Shared.Rent();
            buffer = ArrayPool<byte>.Shared.Rent(4096);
            bufferToOrigin = ArrayPool<byte>.Shared.Rent(4096);

            int readed = readedFromAgent = await nsToAgent.ReadAsync(buffer, 0, 4069);
            httpStreamRequest.WriteAdvance(buffer, 0, readed);

            if (httpStreamRequest.TryPeek(out HttpObject obj))
            {
                var options = obj.fields.ToStartOptions();
                this.nsToAgent = nsToAgent;
                cto = new TcpClient();
                var ct = cto.ConnectAsync(options.HostAddress, options.HostPort);

                this.logger = logger;

                nsToAgent.ReadTimeout = 5000;

                logger.LogInformation($"Выполнил все выделения");

                try
                {
                    await ct;
                    logger.LogWarning($"Соединение с удаленным сервером установлено");
                    nsToOrigin = cto.GetStream();
                    nsToOrigin.WriteTimeout = 15000;

                    //initMsg.request.Write(nsToOrigin);
                    var readedOrigin = await nsToOrigin.ReadAsync(bufferToOrigin, cts.Token);
                    httpStreamRequest.Write(bufferToOrigin, 0, readedOrigin);

                    if (httpStreamRequest.TryAdvance(out var obj2))
                    {
                        if (obj != null)
                        {

                        }
                    }

                    while (true)
                    {
                        await FirstPart();
                        await SecondPart();

                        if (isConnectionClose)
                        {
                            break;
                        }
                    }
                }
                catch (IOException ex)
                {
                    throw ex;
                }
                finally
                {
                    Dispose();
                }
            }
        }

        private async Task FirstPart()
        {
            readedFromAgent = await nsToAgent.ReadAsync(buffer, 0, 4094, cts.Token);
            logger.LogInformation($"Прочитано {readedFromAgent}");

            if (readedFromAgent > 0)
            {
                var task = nsToOrigin.WriteAsync(buffer, 0, readedFromAgent, cts.Token);

                httpStreamRequest.Write(buffer, 0, readedFromAgent);

                if (httpStreamRequest.TryAdvance(out HttpObject obj))
                {
                    if (obj.fields.valuePerFieldName.TryGetValue(HttpLibConstants.FieldNames.NAME_CONNECTION, out var value))
                    {
                        if (value.Contains("Close", StringComparison.InvariantCultureIgnoreCase))
                        {
                            isConnectionClose = true;
                        }
                    }
                }
                await task;
                logger.LogInformation($"Отправлено {readedFromAgent}");
            }
        }

        private async Task SecondPart()
        {

        }


        public void Dispose()
        {
            isConnectionClose = false;
            httpStreamRequest?.TryDispose();
            cto?.Dispose();
            if (buffer != null) ArrayPool<byte>.Shared.Return(buffer);
            if (bufferToOrigin != null) ArrayPool<byte>.Shared.Return(bufferToOrigin);
            if (httpStreamRequest != null) PoolObjectsConcurent<HttpObjectStream>.Shared.Return(httpStreamRequest);
            if (httpStreamResponse != null) PoolObjectsConcurent<HttpObjectStream>.Shared.Return(httpStreamResponse);
            buffer = null;
            httpStreamRequest = null;
            nsToAgent = default;
            nsToOrigin = default;
            if (!cts.TryReset())
            {
                cts = new CancellationTokenSource();
            }
        }
    }
}