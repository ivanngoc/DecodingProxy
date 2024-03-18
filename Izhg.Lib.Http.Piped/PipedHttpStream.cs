using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Async;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Libs.Streaming;
using IziHardGames.Proxy.Consuming;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipelines;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using HttpResult = HttpDecodingProxy.ForHttp.HttpObject;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{

    public class PipedHttpStream : ReusableStream
    {
        private readonly Pipe pipe;
        private readonly PipeReader reader;
        private readonly PipeWriter writer;
        private Stream source;

        public event Action<HttpSource, HttpResult> OnObject;

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }

        private readonly ConcurrentQueue<HttpObject> queue = new ConcurrentQueue<HttpObject>();
        private readonly AsyncAutoResetEvent sync = new AsyncAutoResetEvent();

        public PipedHttpStream() : base()
        {
            pipe = new Pipe();
            reader = pipe.Reader;
            writer = pipe.Writer;
        }
        public PipedHttpStream Init(Stream from)
        {
            this.source = from;
            return this;
        }

        #region Overrides
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            writer.Advance(count);

            throw new NotImplementedException();
        }
        #endregion

        /// <summary>
        /// Представим что поток А пишет данные слева направо а другой поток догоняет считывая данные таким образом съедая данные. 
        /// Съеденые данные 100% уже не понадобятся. Если мы говорим о chunked структуре то можно сказать, что чанки которые заполнились первыми
        /// будут освобождены первыми по мере потребления. Вопрос поддерживает ли такой API Pipe
        /// </summary>
        public void Consume(HttpSource dataSource)
        {
            HttpResult result = null;
            reader.AdvanceTo(new SequencePosition(null, 10));

            if (result != null)
            {
                OnObject?.Invoke(dataSource, result);
            }
            throw new System.NotImplementedException();
        }

        public void CopyFrom(Socket socket)
        {
            throw new System.NotImplementedException();
        }
        public void CopyFrom(NetworkStream stream)
        {
            throw new System.NotImplementedException();
        }
        public void CopyFrom(Stream stream)
        {
            var mem = writer.GetMemory(4096);
            NetworkStream ns = null;
            //stream.Write(mem);
            throw new System.NotImplementedException();
        }
        public async Task CopyOnce(PipedHttpStream streamToOrigin)
        {
            throw new NotImplementedException();
        }
        internal void TryReadHttpMsg()
        {
            throw new NotImplementedException();
        }

        public bool TryDequeueMsg(out HttpObject obj)
        {
            throw new System.NotImplementedException();
        }

        public async Task ParseMessagesAsync(CancellationTokenSource cts, Action<HttpObject> consumer, Stream redirect = null)
        {
            sync.Reset();

            cts.Token.Register(() =>
            {
                pipe.Reader.CancelPendingRead();
                pipe.Writer.CancelPendingFlush();

                if (source is SslStream sslStream)
                {

                }
            }
            );

            var fillPipe = Task.Run(async () =>
             {
                 while (!cts.IsCancellationRequested)
                 {
                     try
                     {
                         var mem = writer.GetMemory(4096);
                         var res = await source.ReadAsync(mem, cts.Token).ConfigureAwait(false);

                         if (res > 0)
                         {
                             writer.Advance(res);
                             var flushResult = await writer.FlushAsync(cts.Token).ConfigureAwait(false);
                         }
                     }
                     catch (OperationCanceledException ex)
                     {
                         return;
                     }
                 }
             });

            var produceMsg = Task.Run(async () =>
            {
                var pool = PoolObjectsConcurent<HttpObject>.Shared;
                HttpObject current = pool.Rent();
                current.BindToPool(pool);
                current.BeginMsg();

                while (!cts.IsCancellationRequested)
                {
                    try
                    {
                        var result = await reader.ReadAsync(cts.Token).ConfigureAwait(false);
                        var buffer = result.Buffer;
                        if (buffer.Length > 0)
                        {
                            int consumed = current.Push(buffer);

                            if (current.isCompleted)
                            {
                                queue.Enqueue(current);
                                sync.Set();
                                current = pool.Rent();
                                current.BindToPool(pool);
                                current.BeginMsg();
                            }
                            if (redirect != null)
                            {
                                var slice = buffer.Slice(0, consumed);

                                if (slice.IsSingleSegment)
                                {
                                    await redirect.WriteAsync(slice.First).ConfigureAwait(false);
                                }
                                else
                                {
                                    foreach (var segment in slice)
                                    {
                                        await redirect.WriteAsync(segment).ConfigureAwait(false);
                                    }
                                }
                            }
                            reader.AdvanceTo(buffer.GetPosition(consumed));
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        return;
                    }
                }
                if (!current.isCompleted) current.Dispose();
            });

            while (true)
            {
                var task = sync.WaitAsync(cts.Token);
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (TaskCanceledException ex)
                {
                    
                }
                if (task.IsCanceled)
                {
                    while (queue.Count > 0)
                    {
                        queue.TryDequeue(out var item);
                        consumer.Invoke(item);
                    }
                    break;
                }
                else
                {
                    HttpObject msg = default;
                    while (!queue.TryDequeue(out msg)) { }
                    consumer.Invoke(msg);

                    if (msg.fields.CheckCloseConnection())
                    {
                        Console.WriteLine($"Finded field close connection");
                        cts.Cancel();
                        goto OUT;
                    }
                }
            }
            OUT:
            await Task.WhenAll(fillPipe, produceMsg).ConfigureAwait(false);
        }

        public void Write(object data, int v, object length)
        {
            throw new NotImplementedException();
        }
    }
}