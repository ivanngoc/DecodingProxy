using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using IziHardGames.Proxy.Datas;
using ProxyLibs.Extensions;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Threading.Tasks;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    /// <summary>
    /// Режим передачи данных через прокси при котором идет одновременное считывание и записывание данных в двух направлениях.
    /// Данные декодируются 2 раза на каждый конец и также кодируются 2 раза для каждого конца.
    /// По сути это посредник (intermediary) между двумя передающими концами. 
    /// </summary>
    public class TwoWayMode : IDisposable
    {
        public const int TYPE_REQUEST = 1;
        public const int TYPE_RESPONSE = 2;

        private Stream agent;
        private Stream origin;

        public const int DEFAULT_BLOCK_SIZE = (1 << 10) * 32; //32 KiB
        private int blockSize = DEFAULT_BLOCK_SIZE;
        private int blockCountRequest;
        private int blockCountResponse;
        private uint countRequest = 0;
        private uint countResponses = 0;

        private uint id;
        private IBlockConsumer[] dataConsumersRequest;
        private IBlockConsumer[] dataConsumersResponse;
        private StartOptions options;
        private DataSource dataSource = new DataSource(typeof(TwoWayMode).Name);
        private bool isDisposed;
        private static uint count;


        public async Task Run(Stream agent, Stream origin, IBlockConsumer[] consumersRequest, IBlockConsumer[] consumersResponse, StartOptions options, int blockSize = DEFAULT_BLOCK_SIZE)
        {
            dataSource.StartNewGeneration();
            var val = Interlocked.Increment(ref count);
            this.id = val;

            this.isDisposed = false;
            this.dataConsumersRequest = consumersRequest;
            this.dataConsumersResponse = consumersResponse;
            this.options = options;

            this.blockSize = blockSize;
            this.agent = agent;
            this.origin = origin;

            var token = options.cts.Token;
            await MaintainConnection(agent, origin, token).ConfigureAwait(false);

            if (false)
            {
                var readFromAgent = Task.Run(async () => await ReadFromAgent(token), token);
                var readFromOrigin = Task.Run(async () => await ReadFromOrigin(token), token);

                var v = await Task.WhenAny(readFromAgent, readFromOrigin).ConfigureAwait(false);
                options.cts.Cancel();
                await Task.WhenAll(readFromAgent, readFromOrigin).ConfigureAwait(false);

                if (readFromAgent.Exception != null) Logger.LogException(readFromAgent.Exception);
                if (readFromOrigin.Exception != null) Logger.LogException(readFromOrigin.Exception);
            }
        }
        private async Task ReadFromOrigin(CancellationToken token)
        {
            Block block = default;
            List<Task> tasks = new List<Task>();
            CancellationTokenSource cts = new CancellationTokenSource();

            var t1 = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested || tasks.Count > 0)
                {
                    if (tasks.Count > 0)
                    {
                        var completed = await Task.WhenAny(tasks).ConfigureAwait(false);

                        lock (tasks)
                        {
                            tasks.Remove(completed);
                        }
                    }
                    await Task.Delay(5000);
                }
            }, cts.Token);

            while (!token.IsCancellationRequested)
            {
                if (block == null)
                {
                    blockCountResponse++;
                    block = Block.Create(blockSize, id, TYPE_RESPONSE, blockCountResponse);
                }

                try
                {
                    var readed = await origin.ReadAsync(block.MemoryRaw).ConfigureAwait(false);

                    if (readed > 0)
                    {
                        Logger.LogLine($"Block Readed From Origin [{options.Host}] length:{readed}| id:{id}|    type:{TYPE_RESPONSE}|   count:{blockCountRequest}");
                        block.SetLength(readed);

                        if (dataConsumersResponse.Length > 0)
                        {
                            block.Use();
                            var t2 = Task.Run(() =>
                               {
                                   for (int i = 0; i < dataConsumersResponse.Length; i++)
                                   {
                                       dataConsumersResponse[i].Consume(block);
                                   }
                               });
                            lock (tasks)
                            {
                                tasks.Add(t2);
                            }
                        }

                        block.Use();
                        try
                        {
                            await agent.WriteAsync(block.Data, 0, block.length).ConfigureAwait(false);
                            Logger.LogLine($"Block Readed From Origin [{options.Host}] length:{block.length}| id:{id}|    type:{TYPE_RESPONSE}|   count:{blockCountRequest}");
                        }
                        catch (IOException ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            block.Unuse();
                            block.TryDispose();
                        }
                        block = default;
                    }
                }
                catch (IOException ex)
                {
                    block.Dispose();
                    throw ex;
                }
            }

            cts.Cancel();
            await Task.WhenAll(t1);
        }
        private async Task ReadFromAgent(CancellationToken token)
        {
            Block block = default;
            List<Task> tasks = new List<Task>();
            CancellationTokenSource cts = new CancellationTokenSource();

            var t1 = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested || tasks.Count > 0)
                {
                    if (tasks.Count > 0)
                    {
                        var completed = await Task.WhenAny(tasks).ConfigureAwait(false);

                        lock (tasks)
                        {
                            tasks.Remove(completed);
                        }
                    }
                    await Task.Delay(5000);
                }
            }, cts.Token);


            while (!token.IsCancellationRequested)
            {
                if (block == null)
                {
                    blockCountRequest++;
                    block = Block.Create(blockSize, id, TYPE_REQUEST, blockCountRequest);
                }
                try
                {
                    var readed = await agent.ReadAsync(block.MemoryRaw).ConfigureAwait(false);

                    if (readed > 0)
                    {
                        Logger.LogLine($"Block Sended To Origin [{options.Host}] Length:{readed}|   id:{id}|    type:{TYPE_REQUEST}|    count:{blockCountRequest}");
                        block.SetLength(readed);
                        //queueAgentToOrigin.Enqueue(block);
                        //readerAgentToOrigin.PushBlock(block);

                        if (dataConsumersRequest.Length > 0)
                        {
                            block.Use();
                            var t2 = Task.Run(() =>
                              {
                                  for (int i = 0; i < dataConsumersRequest.Length; i++)
                                  {
                                      dataConsumersRequest[i].Consume(block);
                                  }
                              });

                            lock (tasks)
                            {
                                tasks.Add(t2);
                            }
                        }
                        block.Use();
                        try
                        {
                            await origin.WriteAsync(block.Data, 0, block.length).ConfigureAwait(false);
                            Logger.LogLine($"Block Sended To Origin [{options.Host}] Length:{block.length}|   id:{id}|    type:{TYPE_REQUEST}|    count:{blockCountRequest}");
                        }
                        catch (IOException ex)
                        {
                            throw ex;
                        }
                        finally
                        {
                            block.Unuse();
                            block.TryDispose();
                        }
                        block = default;
                    }
                }
                catch (IOException ex)
                {
                    block.Dispose();
                    throw ex;
                }
            }

            cts.Cancel();
            await Task.WhenAll(t1);
        }

        private async Task MaintainConnection(Stream agent, Stream origin, CancellationToken token)
        {
            var t1 = ProcessData(agent, origin, token, ConsumeRequest);
            var t2 = ProcessData(origin, agent, token, ConsumeResponse);
            await Task.WhenAll(t1, t2).ConfigureAwait(false);
            Console.WriteLine($"Maintan connection finished");
        }
        private async Task ProcessData(Stream from, Stream to, CancellationToken token, Action<HttpObject> consumer)
        {
            using (PipedHttpStream parser = PoolObjectsConcurent<PipedHttpStream>.Shared.Rent().Init(from))
            {
                await parser.ParseMessagesAsync(options.cts, consumer, to).ConfigureAwait(false);
            }
        }
        private void ConsumeRequest(HttpObject httpObject)
        {
            httpObject.type = HttpLibConstants.TYPE_REQUEST;
            countRequest++;
            httpObject.sequnce = countRequest;
            options.ProviderAs<ConsumingProvider>().consumeRequestMsg(dataSource, httpObject);
        }
        private void ConsumeResponse(HttpObject httpObject)
        {
            httpObject.type = HttpLibConstants.TYPE_RESPONSE;
            countResponses++;
            httpObject.sequnce = countResponses;
            options.ProviderAs<ConsumingProvider>().consumeResponseMsg(dataSource, httpObject);
        }
        /// <summary>
        /// </summary>
        /// <remarks>
        /// Если при чтении будет захвачена часть следующего сообщения то нарушится дальнейшая работа приложения
        /// </remarks>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<HttpObject> AwaitMsg(Stream item, CancellationToken token)
        {
            HttpObject httpObject = PoolObjectsConcurent<HttpObject>.Shared.Rent();
            int capacity = 4096;
            int offset = 0;
            int length = capacity;
            var buffer = ArrayPool<byte>.Shared.Rent(capacity);

            while (!httpObject.isCompleted)
            {
                var readed = await item.ReadAsync(new Memory<byte>(buffer, offset, length)).ConfigureAwait(false);
                var left = httpObject.FillFields(new ArraySegment<byte>(buffer, 0, readed));
                left = httpObject.FillBody(left);

                if (left.Count > 0)
                {
                    offset = left.Count;
                    length = capacity - offset;
                }
            }
            ArrayPool<byte>.Shared.Return(buffer);
            return httpObject;
        }

        public void Dispose()
        {
            dataSource.EndGeneration();

            this.isDisposed = true;

            dataConsumersRequest = default;
            dataConsumersResponse = default;

            blockCountRequest = default;
            blockCountResponse = default;
            id = default;
            options = default;
        }
    }
}