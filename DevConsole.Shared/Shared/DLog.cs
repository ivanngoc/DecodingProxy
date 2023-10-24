using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevConsole.Shared.Consoles;
using Izhg.Libs.Mapping;
using IziHardGames.Libs.Async;
using IziHardGames.Libs.Streams;
using IziHardGames.Libs.Streams.Contracts;
using Microsoft.Extensions.Logging;

namespace DevConsole.Shared.Loggers
{
    public class DLog : ILogger
    {
        private readonly object target;
        private int idChannel;
        private TcpClient tcpClient;
        private NetworkStream stream;
        private Encoding encoding = Encoding.UTF8;
        private ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        private CancellationTokenSource cts;
        private readonly AsyncSignaler asyncSignaler = new AsyncSignaler();
        private LogLevel level;
        private Task t1;
        private int counter;

        public DLog(object target)
        {
            this.target = target;
            this.idChannel = GetHashCode();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string text = formatter.Invoke(state, exception);
            WriteLine(text);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return level == logLevel;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }

        public async Task ConnectAsync(CancellationToken token = default)
        {
            this.tcpClient = await TcpAdapter.ConnectAndAuthorizeAsync(encoding, token).ConfigureAwait(false);
            this.stream = tcpClient.GetStream();
        }

        public async Task Stop()
        {
            cts.Cancel();
            await t1.ConfigureAwait(false); // OperatonCanceldException?
        }
        public async Task<Task> Start()
        {
            cts = new CancellationTokenSource();
            await ConnectAsync(cts.Token).ConfigureAwait(false);
            var token = cts.Token;

            var t1 = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await asyncSignaler.Await(token).ConfigureAwait(false);
                    await Dequeue();
                }
            });
            this.t1 = t1;
            return t1;
        }
        private async Task Dequeue()
        {
            if (queue.TryDequeue(out string text))
            {
                counter++;
                var bytes = encoding.GetBytes(text);
                LogHeader logHeader = new LogHeader(id: counter, order: (uint)counter, EContentType.StringUtf8, length: (uint)bytes.Length, 0, DateTime.Now.Ticks);

                using (var arrayBuffer = LogHeader.FromSelf(logHeader))
                {
                    await stream.WriteAsync(arrayBuffer.array, arrayBuffer.offset, arrayBuffer.length).ConfigureAwait(false);
                }
                await stream.WriteAsync(bytes).ConfigureAwait(false);
                await stream.WriteAsync(StreamForLines.rn).ConfigureAwait(false);
            }
        }
        /// <summary>
        /// Queue To Send
        /// </summary>
        /// <param name="line"></param>
        public void WriteLine(string line)
        {
            queue.Enqueue(line);
            asyncSignaler.Set();
        }
    }
}