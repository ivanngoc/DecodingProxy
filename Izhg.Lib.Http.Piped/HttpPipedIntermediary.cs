using HttpDecodingProxy.ForHttp;
using IziHardGames.Core;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.ObjectsManagment;
using IziHardGames.Proxy.Consuming;
using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Callback = System.Func<IziHardGames.Proxy.Consuming.DataSource, System.Buffers.ReadOnlySequence<byte>, System.Threading.Tasks.Task>;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{

    /// <summary>
    /// Recieving/Transmitting end
    /// </summary>       
    public class HttpPipedIntermediary<THub, TClient> : IDisposable
        where THub : IHub<TClient>
        where TClient : class, IClient, IReader, new()
    {
        public readonly DataSource source = new DataSource(typeof(HttpPipedIntermediary<THub, TClient>).Name);

        private Callback? consumeRequest;
        private Callback? consumeResponse;
        private ConsumingProvider? consumingProvider;
        private IGetOrCreateUnique<string, THub, (string, int)>? manager;
        private bool isDisposed = true;
        private static readonly Callback dummy = (x, y) => Task.CompletedTask;
        private IPoolReturn<HttpPipedIntermediary<THub, TClient>> pool;
        private IChangeNotifier<IConnectionData> monitor;
        public readonly SyncRedirection sync = new SyncRedirection();

        public HttpPipedIntermediary<THub, TClient> Init(ConsumingProvider consumingProvider, IGetOrCreateUnique<string, THub, (string, int)> manager, IPoolReturn<HttpPipedIntermediary<THub, TClient>> pool, Core.IChangeNotifier<IConnectionData> monitor)
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
            return this;
        }
        public async Task Run(TClient agent, CancellationTokenSource cts)
        {
            var fillTask = agent.RunWriterLoop();
            agent.ReportTime($"Awaiting First Msg Begin");
            using (var req = await this.AwaitMsg(HttpLibConstants.TYPE_REQUEST, agent, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared))
            {
                agent.ReportTime($"Awaiting First Msg End");
                var address = req.FindHostAndPortFromField();
                string addressString = $"{address.Item1}:{address.Item2}";
                var hub = manager.GetOrCreate(addressString, address);
                hub.Use();
                hub.SetVersion(req.GetVersionString());
                consumingProvider!.consumeBinaryRequest(this.source, req);
                var taskGetOrigin = hub.GetOrCreateAsync("Origin", PoolObjectsConcurent<TClient>.Shared);
                var origin = await taskGetOrigin.ConfigureAwait(false);
                agent.ReportTime($"MaintainProxyConnection awaited origin");
                origin.ReportTime($"MaintainProxyConnection awaited origin");
                var handleTask = this.MaintainProxyConnection(agent, origin, req, cts);
                try
                {
                    await Task.WhenAll(handleTask).ConfigureAwait(false);
                    agent.ReportTime($"handleTask finished. Status:{handleTask.Status}. Exception:{handleTask.Exception?.Message ?? "OK"}");
                }
                catch (TimeoutException ex)
                {
                    agent.ReportTime($"handleTask finished with exception:{ex.Message}. Trace:{Environment.NewLine}{ex.StackTrace}.{Environment.NewLine}Status:{handleTask.Status}. Exception:{handleTask.Exception?.Message ?? "OK"}");
                }
                cts.Cancel();
                agent.StopWriteLoop();
                await fillTask;
                hub.Unuse();
                hub.Return(origin);
            }
        }
        public async Task MaintainProxyConnection(TClient agent, TClient origin, HttpBinaryMapped firstMsg, CancellationTokenSource cts)
        {
            agent.ReportTime($"MaintainProxyConnection start");
            origin.ReportTime($"MaintainProxyConnection start");
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
                var sendTask = origin.SendAsync(firstMsg.GetMemory(), cts.Token);
                await sendTask.ConfigureAwait(false);
                agent.ReportTime($"MaintainProxyConnection Agent Sended to Origin");
                origin.ReportTime($"MaintainProxyConnection Agent Sended to Origin");

                using (var response = await AwaitMsg(HttpLibConstants.TYPE_RESPONSE, origin, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared).ConfigureAwait(false))
                {
                    agent.ReportTime($"MaintainProxyConnection Recived from origin");
                    origin.ReportTime($"MaintainProxyConnection Recived from origin");
                    var t1 = agent.SendAsync(response.GetMemory(), cts.Token);
                    consumingProvider.consumeBinaryResponse(source, response);
                    await t1.ConfigureAwait(false);
                    agent.ReportTime($"MaintainProxyConnection Sended to agent");
                    origin.ReportTime($"MaintainProxyConnection Sended to agent");
                    agent.ReportTime($"MaintainProxyConnection Ended Message exchange first.");
                    origin.ReportTime($"MaintainProxyConnection Ended Message exchange first.");

                    // check if there next msg
                    if (response.IsCloseRequired)
                    {
                        if (!cts.IsCancellationRequested) cts.Cancel();
                        agent.ReportTime($"MaintainProxyConnection IsCloseRequired");
                        goto END;
                    }
                    else
                    {
                        if (agent.CheckConnectIndirectly() && origin.CheckConnectIndirectly())
                        {
                            var t2 = MaintainMessagingV2(HttpLibConstants.TYPE_REQUEST, agent, origin, cts, consumingProvider.consumeBinaryRequest);
                            var t3 = MaintainMessagingV2(HttpLibConstants.TYPE_RESPONSE, origin, agent, cts, consumingProvider.consumeBinaryResponse);
                            await Task.WhenAll(t2, t3).ConfigureAwait(false);
                            agent.ReportTime($"MaintainProxyConnection End MaintainMessagingV2");
                        }
                        else
                        {
                            agent.ReportTime($"MaintainProxyConnection Closing Agent");
                            goto END;
                        }
                    }
                }
            }
            END:
            agent.ReportTime($"MaintainProxyConnection End");
            origin.ReportTime($"MaintainProxyConnection End");
        }

        public async Task<HttpBinaryMapped> AwaitMsg(int type, TClient client, CancellationTokenSource cts, IPoolObjects<HttpBinaryMapped> pool)
        {
            client.ReportTime($"AwaitMsg Start");
            HttpBinaryMapped item = pool.Rent().Init();
            item.BindToPool(pool, type);

            if (!cts.IsCancellationRequested)
            {
                await AwaitFeilds(item, client, cts.Token).ConfigureAwait(false);
#if DEBUG
                Console.WriteLine(item.ToStringFields());
#endif
                await AwaitBody(item, client, cts.Token).ConfigureAwait(false);
                if (!item.Validate()) throw new FormatException(item.ToString());
                item.ApplyControls(client);

                if (type == HttpLibConstants.TYPE_RESPONSE && item.IsCloseRequired)
                {
                    if (await item.TryReadBodyUntilEnd(client, cts))
                    {
                        //Console.WriteLine(response.debug);
                    }
                }
            }
            client.PutMsg(item);
            client.ReportTime($"AwaitMsg End");
            return item;
        }

        public async Task AwaitFeilds(HttpBinaryMapped item, TClient client, CancellationToken token)
        {
            client.ReportTime($"AwaitFeilds Begin");
            var reader = client;
            item.FieldsStart();
            int offset = 0;
            bool isEnd = default;
            do
            {
                ReadResult result = await reader.ReadAsync(token).ConfigureAwait(false);
                var buffer = result.Buffer;
                if (buffer.Length > 0)
                {
                    isEnd = ParseLineRecursive(item, ref buffer, offset);
                    offset = item.Length;
                }
                else
                {
                    Logger.LogWarning($"{nameof(HttpPipedIntermediary<THub, TClient>)}.{AwaitFeilds} Null read Not implemented");
                }
                reader.ReportConsume(buffer.Start);
            } while (!isEnd);
            client.ReportTime($"AwaitFeilds End");
        }

        // исхлдим их того что строка короткая поэтому если к уже считанному сегменту прибавится еще сегмент
        // то не надо оптимизировать двойное чтение в случае если в сегменте не будет найден конец строки
        // поэтому даже если будет найдено 10 строк и не будет найден с одного чтения \r\n\r\n то издержки производительности будут ничтожеными исходя из количества таких случаев(0,01%)
        /// <summary>
        /// <see cref="HttpObject.Push(ReadOnlySequence{byte})"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns>
        /// <see cref="bool"/> - fields enclosure finded (true)<br/>
        /// <see cref="ReadOnlySequence{T}"/> - latest buffer after last succesfull slicing<br/>
        /// </returns>
        private bool ParseLineRecursive(HttpBinaryMapped item, ref ReadOnlySequence<byte> buffer, int offset)
        {
#if DEBUG
            var s = buffer.ToStringUtf8();
#endif            
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position != null)
            {
                int index = item.FieldsCount;
                var nextPos = buffer.GetPosition(1, position.Value);
                var slice = buffer.Slice(0, nextPos);
                var span = item.AddField(slice, offset);
                buffer = buffer.Slice(nextPos);
                // if empty line
                if (span.Length == HttpLibConstants.LENGTH_LF) // && span[0] == '\r' && span[1] == '\n')  // almost impossible case
                {
                    item.AllocateFieldsMap();
                    item.MapField(index, offset, HttpLibConstants.LENGTH_LF);
                    item.FieldsEnd();
                    return (true);
                }
                // Skip the line + the \n character (basically position)
                var isEnd = ParseLineRecursive(item, ref buffer, offset + span.Length);
                //if (consumed.Item2 > 0) httpObject.MapField(index, offset, span.Length);  // in case of partial reading in few goes
                item.MapField(index, offset, span.Length);
                return (isEnd);
            }
            item.AllocateFieldsMap();
            return false;
        }

        /// <summary>
        /// <see cref="HttpObject.FillBodyChunked(in ReadOnlySequence{byte})"/>
        /// <see cref="HttpBody.ReadBody(Stream, HttpObject)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task AwaitBody<T>(HttpBinaryMapped item, T client, CancellationToken token) where T : IReader, IPerfTracker
        {
            item.BodyStart();
            client.ReportTime($"Await Body Begin");
            int length = item.GetBodyLength();
            var reader = client;
            //ReadResult resultD = await reader.ReadAsync().ConfigureAwait(false);
            //var bufferD = resultD.Buffer;
            //string debug = Encoding.UTF8.GetString(bufferD);

            if (length > 0)
            {
                item.EnsureCapacity(length);

                while (length > 0)
                {
                    ReadResult result = await reader.ReadAsync(token).ConfigureAwait(false);

                    var buffer = result.Buffer;

                    if (length > buffer.Length)
                    {
                        length -= (int)buffer.Length;
                        item.AddBody(buffer);
                        reader.ReportConsume(buffer.End);
                    }
                    else
                    {
                        SequencePosition position = buffer.GetPosition(length);
                        var slice = buffer.Slice(0, length);
                        item.AddBody(slice);
                        reader.ReportConsume(position);
                        item.BodyEnd();
                        goto END;
                    }
                }
            }
            else if (length == -2)
            {
                await ReadBodyChunkedV1(reader, item, token).ConfigureAwait(false);

            }
            else if (length == -3)
            {
                item.isReadingUntilCloseConnection = true;
                item.BodyEnd();
                goto END;
            }
            END:
            client.ReportTime($"Await Body End");
        }

        /// <summary>
        /// Read c <see cref="PipedSocket.Consume(int)"/>
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="item"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private async Task ReadBodyChunkedV2(TcpClientPiped reader, HttpBinaryMapped item, CancellationToken token)
        {
            await Task.Delay(1000);
            throw new System.NotImplementedException();
        }
        private async Task ReadBodyChunkedV1<T>(T reader, HttpBinaryMapped item, CancellationToken token) where T : IReader
        {
            SequencePosition? pos;
            SequencePosition position;
            bool isReadingChunk = false;
            bool isReadingControl = true;
            bool isReadingEnclosure = false;
            bool isEnd = false;
            int lengthLeft = 0;

            while (true)
            {
                var result = await reader.ReadAsync(token).ConfigureAwait(false);
                var buffer = result.Buffer;
                int lengthLoop = buffer.Length < lengthLeft ? (int)buffer.Length : lengthLeft;
                if (isReadingControl)
                {   // search control line ending
                    pos = buffer.PositionOf((byte)'\n');

                    if (pos == null)
                    {
                        continue;
                    }
                    isReadingControl = false;
                    var nextPos = buffer.GetPosition(1, pos.Value);
                    var controline = buffer.Slice(0, nextPos);
                    // add control line to container
                    item.AddBodyForSure(controline);
                    buffer = buffer.Slice(controline.Length);
                    int chunkLength = controline.ParseHex();
                    if (chunkLength == 0)
                    {
                        isReadingEnclosure = true;
                        isEnd = true;
                        item.BodyEnd();
                        goto END;
                    }
                    else
                    {
                        isReadingChunk = true;
                        lengthLeft = chunkLength;
                        lengthLoop = chunkLength > buffer.Length ? (int)buffer.Length : chunkLength;
                    }
                }

                if (isReadingChunk)
                {
                    var sliceChunk = buffer.Slice(0, lengthLoop);
                    // add chunked body to container
                    item.AddBodyForSure(sliceChunk);
                    buffer = buffer.Slice(lengthLoop);
                    lengthLeft -= lengthLoop;
                    if (lengthLeft == 0) isReadingEnclosure = true;
                    isReadingChunk = !isReadingEnclosure;
                }
                END:
                if (isReadingEnclosure)
                {
                    // read CRLF after chunk and add to container
                    var lf = buffer.Slice(0, HttpLibConstants.LENGTH_LF);
                    item.AddBodyForSure(lf);
                    buffer = buffer.Slice(HttpLibConstants.LENGTH_LF);
                    isReadingControl = true;
                    isReadingEnclosure = false;
                    if (isEnd)
                    {
                        position = buffer.GetPosition(0);
                        reader.ReportConsume(position);
                        break;
                    }
                }
                position = buffer.GetPosition(0);
                reader.ReportConsume(position);
            }
        }
        public void Dispose()
        {
            if (this.isDisposed) throw new InvalidOperationException($"");
            this.isDisposed = true;
            source.EndGeneration();
            consumeRequest = dummy;
            consumeResponse = dummy;
            sync.Dispose();

            pool.Return(this);
            pool = default;

            monitor = default;
            this.manager = default;
            this.consumingProvider = default;
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

        public async Task MaintainMessagingSequentially(int type, TClient from, TClient to, CancellationTokenSource cts, Action<DataSource, HttpBinaryMapped> consumer)
        {
            from.ReportTime($"MaintainMessaging Begin");
            int messages = default;
            while (!cts.IsCancellationRequested)
            {
                var t1 = AwaitMsg(type, from, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared);
                var msg = await t1.ConfigureAwait(false);
                messages++;
                if (t1.IsCanceled) goto END;
                from.ReportTime($"Message #{messages} recived");

                if (type == HttpLibConstants.TYPE_RESPONSE)
                {
                    from.ConsumeLife();
                    if (msg.IsCloseRequired)
                    {
                        from.ReportTime($"MaintainMessaging is Close required");
                        cts.Cancel();
                        goto END;
                    }
                }
                else
                {
                    to.ReportTime($"Message #{messages} recived");
                }
                consumer(source, msg);
                var t2 = to.SendAsync(msg.GetMemory(), cts.Token);
                await t2;
                from.ReportTime($"Message #{messages} sended");
                ReportMessageEnd(type, from, to, cts);
            }
            END:
            from.ReportTime($"MaintainMessaging End");
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
        public async Task MaintainMessagingV2(int type, TClient from, TClient to, CancellationTokenSource cts, Action<DataSource, HttpBinaryMapped> consumer)
        {
            var sync = this.sync;
            AsyncAutoResetEvent<HttpBinaryMapped> are = PoolObjectsConcurent<AsyncAutoResetEvent<HttpBinaryMapped>>.Shared.Rent();
            //возможно сервер отправляет данные до поступления запроса на него по принципу детерминированности
            var read = Task.Run(async () =>
            {
                try
                {
                    int counter = default;
                    if (!from.CheckConnectIndirectly() && !to.CheckConnectIndirectly())
                    {
                        are.Cancel();
                        from.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. CheckConnectIndirectly() is False. Finish Task");
                        return;
                    }
                    do
                    {
#if DEBUG
                        //Console.WriteLine($"MaintainMessagingV2.Type:{type}. From:{from.Title} TO:{to.Title} recived #{counter}. Available:{from.Available}. Begin await msg to recieve");
                        //Console.WriteLine(msg.ToStringFields());
#endif
                        var msg = await AwaitMsg(type, from, cts, PoolObjectsConcurent<HttpBinaryMapped>.Shared).ConfigureAwait(false);
                        counter++;
                        from.ReportTime($"MaintainMessagingV2.Type:{type}. From:{from.Title} TO:{to.Title} recived #{counter}. Available:{from.Available}");
                        // check for next request
                        if (type == HttpLibConstants.TYPE_REQUEST)
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
                    from.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. read task ended");
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
                        var task = to.SendAsync(res.GetMemory(), cts.Token);
                        consumer(source, res);
                        await task.ConfigureAwait(false);
                        from.ReportTime($"MaintainMessagingV2 sended. From:{from.Title} TO:{to.Title} #{counter}. Available:{from.Available}");

                        if (type == HttpLibConstants.TYPE_REQUEST)
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
                            from.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. sending task ended by required closing connection. CTS.Cancel()");
                            cts.Cancel();
                            return;
                        }
                    }
                }
                catch (TaskCanceledException ex)
                {
                    from.ReportTime($"MaintainMessagingV2. From:{from.Title} TO:{to.Title}. send task canceled exception: Trace:{Environment.NewLine}{ex.StackTrace}.{Environment.NewLine}ExceptionMessage:{ex.Message}");
                }
            });
            await Task.WhenAll(read, send).ConfigureAwait(false);
            from.ReportTime($"MaintainMessagingV2 Ended. From:{from.Title} TO:{to.Title}");
            are.Reset();
            PoolObjectsConcurent<AsyncAutoResetEvent<HttpBinaryMapped>>.Shared.Return(are);
        }
        private void ReportMessageEnd(int type, TClient from, TClient to, CancellationTokenSource cts)
        {
            if (type == HttpLibConstants.TYPE_RESPONSE)
            {
                if (from.Available == 0)
                {
                    from.ReportTime($"ReportMessageEnd TYPE_RESPONSE available=0");
                    cts.Cancel();
                }
            }
        }


    }
}