using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Core;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.ForHttp.Http11;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.ObjectsManagment;
using IziHardGames.Proxy.Consuming;
using Callback = System.Func<IziHardGames.Proxy.Consuming.HttpSource, System.Buffers.ReadOnlySequence<byte>, System.Threading.Tasks.Task>;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{

    /// <summary>
    /// Recieving/Transmitting end
    /// </summary>       
    public class HttpPipedIntermediary<THub, TClient> : IDisposable
        where THub : IHub<TClient>
        where TClient : class, IClient<SocketReader, SocketWriter>, IReader, new()  // piped client
    {
        public readonly HttpSource source = new HttpSource(typeof(HttpPipedIntermediary<THub, TClient>).Name);

        private Callback? consumeRequest;
        private Callback? consumeResponse;
        private ConsumingProvider? consumingProvider;
        private IGetOrCreateUnique<string, THub, (string, int)>? manager;
        private bool isDisposed = true;
        private static readonly Callback dummy = (x, y) => Task.CompletedTask;
        private IPoolReturn<HttpPipedIntermediary<THub, TClient>>? pool;
        private IChangeNotifier<IConnectionData>? monitor;
        public readonly SyncRedirection sync = new SyncRedirection();

        private TClient? client;
        private ObjectReaderHttp11Piped? readerRequest;
        private ObjectReaderHttp11Piped? readerResponse;

        public HttpPipedIntermediary<THub, TClient> Init(ConsumingProvider consumingProvider,
                                                         IGetOrCreateUnique<string, THub, (string, int)> manager,
                                                         IPoolReturn<HttpPipedIntermediary<THub, TClient>> pool,
                                                         Core.IChangeNotifier<IConnectionData> monitor)
        {
            if (!isDisposed) throw new InvalidOperationException($"Object Must be disposed or use after created");
            isDisposed = false;
            this.pool = pool;
            this.manager = manager;
            this.consumingProvider = consumingProvider;
            this.consumeRequest = consumingProvider!.consumeRequest;
            this.consumeResponse = consumingProvider!.consumeResponse;
            source.StartNewGeneration();
            this.monitor = monitor;
            readerRequest = PoolObjectsConcurent<ObjectReaderHttp11Piped>.Shared.Rent();
            readerResponse = PoolObjectsConcurent<ObjectReaderHttp11Piped>.Shared.Rent();
            return this;
        }
        public async Task Run(TClient agent, CancellationTokenSource cts)
        {

            var poolItems = PoolObjectsConcurent<HttpBinaryMapped>.Shared;
            readerRequest!.Initilize(agent, ConstantsForHttp.TYPE_REQUEST, cts, poolItems);

            IPerfTracker logger = agent.Logger as IPerfTracker ?? throw new NullReferenceException();

            var pipedClient = agent as IClientPiped<SocketReader, SocketWriter>;
            Task fillTask = pipedClient?.RunWriterLoop() ?? Task.CompletedTask;

            logger.ReportTime($"Awaiting First Msg Begin");
            using (var req = await this.AwaitMsg(readerRequest, cts, poolItems))
            {
                logger.ReportTime($"Awaiting First Msg End");
                var address = req.FindHostAndPortFromField();
                string addressString = $"{address.Item1}:{address.Item2}";
                var hub = manager!.GetOrCreate(addressString, address);
                hub.Use();
                hub.SetVersion(req.GetVersionString());
                consumingProvider!.consumeBinaryRequest(this.source, req);
                var taskGetOrigin = hub.GetOrCreateAsync(EConnectionFlags.None, "Origin", PoolObjectsConcurent<TClient>.Shared);
                var origin = await taskGetOrigin.ConfigureAwait(false);
                logger.ReportTime($"MaintainProxyConnection awaited origin");
                var handleTask = this.MaintainProxyConnection(agent, origin, req, cts);
                try
                {
                    await Task.WhenAll(handleTask).ConfigureAwait(false);
                    logger.ReportTime($"handleTask finished. Status:{handleTask.Status}. Exception:{handleTask.Exception?.Message ?? "OK"}");
                }
                catch (TimeoutException ex)
                {
                    logger.ReportTime($"handleTask finished with exception:{ex.Message}. Trace:{Environment.NewLine}{ex.StackTrace}.{Environment.NewLine}Status:{handleTask.Status}. Exception:{handleTask.Exception?.Message ?? "OK"}");
                }
                cts.Cancel();
                pipedClient?.StopWriteLoop();
                await fillTask;
                hub.Unuse();
                hub.Return(origin);
            }
        }
        public async Task MaintainProxyConnection(TClient agent, TClient origin, HttpBinaryMapped firstMsg, CancellationTokenSource cts)
        {
            var poolItems = PoolObjectsConcurent<HttpBinaryMapped>.Shared;
            readerResponse.Initilize(origin, ConstantsForHttp.TYPE_RESPONSE, cts, poolItems);

            IPerfTracker logger = agent.Logger as IPerfTracker ?? throw new NullReferenceException();
            logger.ReportTime($"MaintainProxyConnection start");
            logger.ReportTime($"MaintainProxyConnection start");
            var pool = PoolObjectsConcurent<TClient>.Shared;
            CancellationToken token = cts.Token;
            {
                //string getstring =
                //    //"GET http://192.168.0.1/ HTTP/1.1\r\n" +
                //    "GET / HTTP/1.1\r\n" +
                //    "Host: 192.168.0.1\r\n" +
                //    "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/114.0\r\n" +
                //    "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\n" +
                //    "Accept-Language: ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3\r\n" +
                //    "Accept-Encoding: gzip, deflate\r\n" +
                //    "DNT: 1\r\n" +
                //    "Connection: keep-alive\r\n" +
                //    "Upgrade-Insecure-Requests: 1\r\n" +
                //    "Pragma: no-cache\r\n" +
                //    "Cache-Control: no-cache\r\n" +
                //    "\r\n";
                //var bb = Encoding.UTF8.GetBytes(firstMsg.Fields.Substring(0, firstMsg.Length - 2));
                //var bb = Encoding.UTF8.GetBytes(firstMsg.Fields + "\r\n");
                //var bb = Encoding.UTF8.GetBytes(getstring);
                //var sendTask = origin.SendAsync(bb);
                //origin.Send(bb);
                var sendTask = origin.Writer.WriteAsync(firstMsg.GetMemory(), cts.Token);
                await sendTask.ConfigureAwait(false);
                logger.ReportTime($"MaintainProxyConnection Agent Sended to Origin");

                using (var response = await AwaitMsg(readerResponse, cts, poolItems).ConfigureAwait(false))
                {
                    logger.ReportTime($"MaintainProxyConnection Recived from origin");
                    var t1 = agent.Writer.WriteAsync(response.GetMemory(), cts.Token);
                    consumingProvider.consumeBinaryResponse(source, response);
                    await t1.ConfigureAwait(false);
                    logger.ReportTime($"MaintainProxyConnection Sended to agent");
                    logger.ReportTime($"MaintainProxyConnection Ended Message exchange first.");

                    // check if there next msg
                    if (response.IsCloseRequired)
                    {
                        if (!cts.IsCancellationRequested) cts.Cancel();
                        logger.ReportTime($"MaintainProxyConnection IsCloseRequired");
                        goto END;
                    }
                    else
                    {
                        if (agent.CheckConnectIndirectly() && origin.CheckConnectIndirectly())
                        {
                            var t2 = MaintainMessagingV2(readerRequest, agent, origin, cts, consumingProvider.consumeBinaryRequest);
                            var t3 = MaintainMessagingV2(readerResponse, origin, agent, cts, consumingProvider.consumeBinaryResponse);
                            await Task.WhenAll(t2, t3).ConfigureAwait(false);
                            logger.ReportTime($"MaintainProxyConnection End MaintainMessagingV2");
                        }
                        else
                        {
                            logger.ReportTime($"MaintainProxyConnection Closing Agent");
                            goto END;
                        }
                    }
                }
            }
            END:
            logger.ReportTime($"MaintainProxyConnection End");
        }


        public async Task<HttpBinaryMapped> AwaitMsg(ObjectReaderHttp11Piped reader, CancellationTokenSource cts, IPoolObjects<HttpBinaryMapped> pool)
        {
            var result = await reader.ReadObjectAsync();
            var item = result.value;

            if (!item.Validate()) throw new FormatException(item.ToString());
            item.ApplyControls(client!);

            if (reader.Type == ConstantsForHttp.TYPE_RESPONSE && item.IsCloseRequired)
            {
                if (await item.TryReadBodyUntilEnd(client!, cts))
                {
                    //Console.WriteLine(response.debug);
                }
            }
            return item;
        }




        public void Dispose()
        {
            if (this.isDisposed) throw new InvalidOperationException($"");
            this.isDisposed = true;
            source.EndGeneration();
            consumeRequest = dummy;
            consumeResponse = dummy;
            sync.Dispose();

            pool!.Return(this);
            pool = default;

            monitor = default;
            this.manager = default;
            this.consumingProvider = default;

            client = default;

            readerRequest!.Dispose();
            readerResponse!.Dispose();
            PoolObjectsConcurent<ObjectReaderHttp11Piped>.Shared.Return(readerRequest!);
            PoolObjectsConcurent<ObjectReaderHttp11Piped>.Shared.Return(readerResponse!);

            readerRequest = default;
            readerResponse = default;
        }
        public void Resume(int readed)
        {
            throw new NotImplementedException();
        }
        public bool TryParseFields()
        {
            throw new System.NotImplementedException();
        }

        string GetAsciiString(ReadOnlySequence<byte> buffer)
        {
            if (buffer.IsSingleSegment)
            {
                return Encoding.ASCII.GetString(buffer.First.Span);
            }

            return string.Create((int)buffer.Length, buffer, (span, sequence) =>
            {
                foreach (var segment in sequence)
                {
                    Encoding.ASCII.GetChars(segment.Span, span);
                    span = span.Slice(segment.Length);
                }
            });
        }

        public async Task MaintainMessagingSequentially(ObjectReaderHttp11Piped producer, TClient from, TClient to, CancellationTokenSource cts, Action<HttpSource, HttpBinaryMapped> consumer)
        {
            IPerfTracker logger = from.Logger as IPerfTracker ?? throw new NullReferenceException();

            logger.ReportTime($"MaintainMessaging Begin");
            int messages = default;
            while (!cts.IsCancellationRequested)
            {
                var t1 = AwaitMsg(producer, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared);
                var msg = await t1.ConfigureAwait(false);
                messages++;
                if (t1.IsCanceled) goto END;
                logger.ReportTime($"Message #{messages} recived");

                if (producer.Type == ConstantsForHttp.TYPE_RESPONSE)
                {
                    from.ConsumeLife();
                    if (msg.IsCloseRequired)
                    {
                        logger.ReportTime($"MaintainMessaging is Close required");
                        cts.Cancel();
                        goto END;
                    }
                }
                else
                {
                    logger.ReportTime($"Message #{messages} recived");
                }
                consumer(source, msg);
                var t2 = to.Writer.WriteAsync(msg.GetMemory(), cts.Token);
                await t2;
                logger.ReportTime($"Message #{messages} sended");
                ReportMessageEnd(producer.Type, from, to, cts);
            }
            END:
            logger.ReportTime($"MaintainMessaging End");
        }

        /// <summary>
        /// Передача сообщения с разделенным чтением и переотправкой
        /// </summary>
        /// <param name="type"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cts"></param>
        /// <param name="consumer"></param>
        /// <returns></returns>
        public async Task MaintainMessagingV2(ObjectReaderHttp11Piped producer, TClient from, TClient to, CancellationTokenSource cts, Action<HttpSource, HttpBinaryMapped> consumer)
        {
            IPerfTracker logger = from.Logger as IPerfTracker ?? throw new NullReferenceException();

            var sync = this.sync;
            AsyncAutoResetEvent<HttpBinaryMapped> are = PoolObjectsConcurent<AsyncAutoResetEvent<HttpBinaryMapped>>.Shared.Rent();
            //возможно сервер отправляет данные до поступления запроса на него по принципу детерминированности
            int type = producer.Type;

            var read = Task.Run(async () =>
            {
                try
                {
                    int counter = default;
                    if (!from.CheckConnectIndirectly() && !to.CheckConnectIndirectly())
                    {
                        are.Cancel();
                        logger.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. CheckConnectIndirectly() is False. Finish Task");
                        return;
                    }
                    do
                    {
#if DEBUG
                        //Console.WriteLine($"MaintainMessagingV2.Type:{type}. From:{from.Title} TO:{to.Title} recived #{counter}. Available:{from.Available}. Begin await msg to recieve");
                        //Console.WriteLine(msg.ToStringFields());
#endif
                        var msg = await AwaitMsg(producer, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared).ConfigureAwait(false);
                        counter++;
                        logger.ReportTime($"MaintainMessagingV2.Type:{type}. From:{from.Title} TO:{to.Title} recived #{counter}. Available:{from.Available}");
                        // check for next request
                        if (type == ConstantsForHttp.TYPE_REQUEST)
                        {
                            sync.IncrementRecieve();
                            // release connection if there is not data
                            if (!from.CheckData())
                            {
#if DEBUG
                                //Console.WriteLine($"{from.Title}-to-{to.Title} CheckData=false. Release connection");
#endif
                                sync.isRecievingClientToOriginCompleted = true;
                                are.SetLast(msg);
                                goto END;
                            }
                        }
                        else
                        {
                            int val = sync.DecrementRecieve();
                            if (sync.isRecievingClientToOriginCompleted)
                            {
                                var isEnd = sync.isRecievingOriginToClientCompleted = val == 0;
                                if (isEnd)
                                {
                                    are.SetLast(msg);
                                    goto END;
                                }
                            }
                        }
                        are.Set(msg);
                    }
                    while (from.CheckConnectIndirectly());
                    END:
                    logger.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. read task ended");
                }
                catch (TimeoutException)
                {
                    are.Cancel();
                }
            });

            var send = Task.Run(async () =>
            {
                int counter = default;
                try
                {
                    while (!cts.IsCancellationRequested)
                    {
#if DEBUG
                        //Console.WriteLine($"MaintainMessagingV2.Type:{type}. From:{from.Title} TO:{to.Title} recived #{counter}. Available:{from.Available}. Begin await msg to resend");
                        //Console.WriteLine(msg.ToStringFields());
#endif
                        var res = await are.WaitAsync(cts.Token).ConfigureAwait(false);
                        counter++;
                        var task = to.Writer.WriteAsync(res.GetMemory(), cts.Token);
                        consumer(source, res);
                        await task.ConfigureAwait(false);
                        logger.ReportTime($"MaintainMessagingV2 sended. From:{from.Title} TO:{to.Title} #{counter}. Available:{from.Available}");

                        if (type == ConstantsForHttp.TYPE_REQUEST)
                        {
                            sync.IncrementResend();
#if DEBUG
                            //Console.WriteLine($"{from.Title}-to-{to.Title} ARE Check.");
#endif
                            if (are.Count == 0 && are.IsComplete)
                            {
#if DEBUG
                                //Console.WriteLine($"{from.Title}-to-{to.Title} ARE Completed.");
#endif
                                sync.isResendingClientToOriginCompleted = true;
                                return;
                            }
                        }
                        else
                        {
                            int val = sync.DecrementResend();
                            if (sync.isResendingClientToOriginCompleted)
                            {
                                var isEnd = sync.isResendingOriginToClientCompleted = val == 0;
                                if (isEnd)
                                {
#if DEBUG
                                    //Console.WriteLine($"{from.Title}-to-{to.Title} ARE Completed.");
#endif
                                    return;
                                }
                            }
                        }
                        if (res.IsCloseRequired)
                        {
                            logger.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. sending task ended by required closing connection. CTS.Cancel()");
                            cts.Cancel();
                            return;
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    logger.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. send task canceled exception: Trace:{Environment.NewLine}{ex.StackTrace}.{Environment.NewLine}ExceptionMessage:{ex.Message}");
                }
            });
            await Task.WhenAll(read, send).ConfigureAwait(false);
            logger.ReportTime($"MaintainMessagingV2 Ended. From:{from.Title} TO:{to.Title}");
            are.Reset();
            PoolObjectsConcurent<AsyncAutoResetEvent<HttpBinaryMapped>>.Shared.Return(are);
        }
        private void ReportMessageEnd(int type, TClient from, TClient to, CancellationTokenSource cts)
        {
            IPerfTracker logger = from.Logger as IPerfTracker ?? throw new NullReferenceException();

            if (type == ConstantsForHttp.TYPE_RESPONSE)
            {
                if (from.Available == 0)
                {
                    logger.ReportTime($"ReportMessageEnd TYPE_RESPONSE available=0");
                    cts.Cancel();
                }
            }
        }
    }
}