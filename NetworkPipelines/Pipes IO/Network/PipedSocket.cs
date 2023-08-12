using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.DevTools;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Libs.Streaming;
using ProxyLibs.Extensions;

namespace IziHardGames.Libs.Networking.Pipelines
{

    /*
Схема работы: FillPipe => ReadAsync => Parse => Apply Controls => Analyz Continuation => Break/Continue 
*/
    [Obsolete]
    public class PipedSocket : PipedStream
    {
        public Stopwatch stopwatch = new Stopwatch();
        public string Title => title;
        protected string title;
        protected int id;
        protected int generation;
        protected int deaths;
        protected EConnectionState state = EConnectionState.None;
        protected EConnectionFlags flags = EConnectionFlags.Reseted;

        protected Socket socket;
        protected IPEndPoint ipEndPoint;
        protected string host;
        protected int port;

        private uint totalBytesFilled;
        private uint totalBytesConsumed;
        private int readSizeMin = int.MaxValue;
        private int readSizeMax = int.MinValue;
        private int countFills;
        public int Available => available;
        private int available;
        private int position; // reader's position at last consuming report
        private ReadOnlySequence<byte> buffer;
        private ReadOnlySequence<byte> bufferCurrent;   // то что осталось с момента последнего чтения
        private ReadOnlySequence<byte> slice;   // то что было в последний раз отрезано
        public ref ReadOnlySequence<byte> Slice => ref slice;
        public ref ReadOnlySequence<byte> Buffer => ref buffer;
        public ref ReadOnlySequence<byte> BufferCurrent => ref bufferCurrent;
        public ref ReadResult Result => ref result;
        private ReadResult result;

        private bool isTimeoutSetForSend;
        private bool isTimeoutSetForRecieve;

        private int timeoutSend;
        private int timeoutRecieve;

        private int timeoutSendDefault;
        private int timeoutRecieveDefault;
        public bool IsConnected => socket?.Connected ?? false;
        public string Host { get => host; set => host = value; }
        public int Port { get => port; set => port = value; }
        public int Id { get => id; set => id = value; }
        public int Action { get; set; }
        public string Version { get => $"GEN:{generation}. DEATH:{deaths}. TITLE:{Title}"; set => throw new System.NotImplementedException(); }
        public string Status { get => state.ToString(); set => throw new System.NotImplementedException(); }
        public Task LoopWriter => loopWriter;
        private Task loopWriter;
        private Task loopReader;

        private CancellationTokenSource ctsWrite;
        private CancellationTokenSource ctsRead;
        public readonly NetworkLogger logger = new NetworkLogger();

        public DateTime timeConnect;
        public DateTime timeConnectCheck;
        public DateTime timeDisconnectDetected;
        public DateTime timeDispose;
        public int lifetime;
#if DEBUG
        private SocketDebugger debugger = new SocketDebugger();
#endif
        public override void Close()
        {
            ReportTime($"Dispose called. host:{host}, port:{port}");
            timeDispose = DateTime.Now;
            lifetime = (timeDispose - timeConnect).Seconds;
            if (loopWriter != null && !loopWriter.IsCompleted) throw new InvalidOperationException($"Filling pipe is still running");
            deaths++;
            if (isDisposed) throw new ObjectDisposedException($"This object is already Disposed");
            isDisposed = true;
            ReportTime($"Socket closed. lifetime:{lifetime} seconds. timeConnect:{timeConnect.ToString("yyyy.MM.dd HH:mm:ss.ffffff")}. timeDisconnectDetected:{timeDisconnectDetected}");
            if (socket.Connected) socket.Disconnect(true);

            this.state = EConnectionState.Disposed;
            this.flags = EConnectionFlags.Reseted;

            socket.Close(); // dispose called inside
            base.Close();
            ResetPipes();

            ipEndPoint = default;
            port = default;
            host = default;
            socket = default;

            loopReader = default;
            loopWriter = default;
            position = default;
            totalBytesFilled = default;
            totalBytesConsumed = default;

            buffer = default;
            bufferCurrent = default;
            slice = default;
            result = default;

            isTimeoutSetForRecieve = default;
            isTimeoutSetForSend = default;
            timeoutRecieve = default;
            timeoutSend = default;
            timeoutSendDefault = default;
            timeoutRecieveDefault = default;
            readSizeMin = int.MaxValue;
            readSizeMax = int.MinValue;
            countFills = default;
            stopwatch.Reset();
            LogsToFile();
        }

        public void ResetPipes()
        {
            writer.Complete();
            reader.Complete();
            pipe.Reset();
            available = default;
        }
        protected void BindTitle(string title)
        {
            this.title = title;
        }
        protected void BindSocket(Socket socket)
        {
            this.socket = socket;
        }
        public void Initilize(string title)
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed to be reused");
            isDisposed = false;
            generation++;
            stopwatch.Start();
            id = GetHashCode();
            SetDefaultTimeouts(60000, 6000);
            BindTitle(title);
#if DEBUG
            debugger.Init(id, title);
#endif
        }
        public async Task ConnectAsyncTcp(string host, int port)
        {
            ReportTime($"ConnectAsync Start");
            this.host = host;
            this.port = port;
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.DualMode = true;
            //socket.ExclusiveAddressUse = true;
            socket.ReceiveBufferSize = (1 << 20) * 8;
            socket.SendBufferSize = (1 << 20) * 8;
            await socket.ConnectAsync(host, port).ConfigureAwait(false);
            ReportTime($"ConnectAsync End");
            timeConnect = DateTime.Now;
        }
        public void ReportConsume(SequencePosition position)
        {
            var reader = Reader;
            bufferCurrent = bufferCurrent.Slice(position);
            reader.AdvanceTo(position, position);
            int newPos = position.GetInteger();
            int consumed = newPos - this.position;
            ReportConsume(consumed);
            this.position = newPos;
            //Logger.LogInformation($"{guid}: Consumed:{consumed}. Total:{totalBytesConsumed}. Available:{available}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReportConsume(long consumed)
        {
            ReportConsume((int)consumed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ReportConsume(int consumed)
        {
            totalBytesConsumed += (uint)consumed;
            Interlocked.Add(ref available, -consumed);
            ReportTime($"ReportConsume: consumed:{consumed}. Available:{available}");
        }

        public ref ReadOnlySequence<byte> Consume(int length)
        {
            this.slice = bufferCurrent.Slice(0, length);
            bufferCurrent = bufferCurrent.Slice(length);
            ReportConsume(slice.End);
            return ref slice;
        }

        public override void Flush()
        {
            // The Flush method implements the Stream.Flush method; however, because NetworkStream is not buffered, it has no effect on network streams
            // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream.flush?view=net-7.0
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            int sended = socket.Send(buffer, offset, count, SocketFlags.None);
            //Console.WriteLine($"Wrte into tcpCLientPipedSsl. {Environment.NewLine}{Environment.StackTrace}{Environment.NewLine}Value: {Encoding.UTF8.GetString(buffer, offset, count)}");
        }
        public override int Read(byte[] buffer, int offset, int count)
        {   // 
            int readed = socket.Receive(buffer, offset, count, SocketFlags.None);
            Console.WriteLine($"{nameof(PipedSocket)}.{nameof(Read)}() Read. {Environment.NewLine}{Environment.StackTrace}{Environment.NewLine}Value: {Encoding.UTF8.GetString(buffer, offset, readed)}");
            return readed;
        }

        public void Send(byte[] bytes)
        {
            lock (this)
            {
                socket.Send(bytes);
            }
        }

        public void Send(ReadOnlyMemory<byte> bytes)
        {
            lock (this)
            {
                socket.Send(bytes.ToArray());
            }
        }
        public async Task SendAsync(ReadOnlySequence<byte> sequence, CancellationToken token)
        {
            foreach (var segment in sequence)
            {
                if (isTimeoutSetForSend)
                {
                    await socket.SendAsync(segment, SocketFlags.None).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSend)).ConfigureAwait(false);
                }
                else
                {
                    await socket.SendAsync(segment, SocketFlags.None).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSendDefault)).ConfigureAwait(false);
                }
            }
        }
        public async Task SendAsync(ReadOnlyMemory<byte> bytes, CancellationToken token)
        {
            if (isTimeoutSetForSend)
            {
                await socket.SendAsync(bytes, SocketFlags.None).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSend)).ConfigureAwait(false);
            }
            else
            {
                await socket.SendAsync(bytes, SocketFlags.None).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSendDefault)).ConfigureAwait(false);
            }

        }
        public async Task SendAsync(byte[] buffer, int offset, int length, CancellationToken token)
        {
            if (isTimeoutSetForSend)
            {
                await socket.SendAsync(new ReadOnlyMemory<byte>(buffer, offset, length), SocketFlags.None, token).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSend)).ConfigureAwait(false);
            }
            else
            {
                await socket.SendAsync(new ReadOnlyMemory<byte>(buffer, offset, length), SocketFlags.None, token).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSendDefault)).ConfigureAwait(false);
            }
        }
        public async Task SendAsync(Memory<byte> memory, CancellationToken token)
        {
            ReportTime($"SendAsync Start");
            if (isTimeoutSetForSend)
            {
                await socket.SendAsync(memory, SocketFlags.None).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSend)).ConfigureAwait(false);
            }
            else
            {
                await socket.SendAsync(memory, SocketFlags.None).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutSendDefault)).ConfigureAwait(false);
            }
            ReportTime($"SendAsync End");
        }
        public Task RunWriterLoop()
        {
            ctsWrite = new CancellationTokenSource();
            this.loopWriter = Task.Run(async () => await FillPipeAsync(ctsWrite));
            return this.loopWriter;
        }
        public Task StopWriteLoop()
        {
            ctsWrite.Cancel();
            if (!ctsWrite.TryReset())
            {
                ctsWrite = new CancellationTokenSource();
            }
            return loopWriter;
        }
        private async Task FillPipeAsync(CancellationTokenSource cts)
        {
            int bytesRead = default;
            ReportTime($"FillPipeAsync Start");
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    ReportTime($"FillPipeAsync Start Loop available: socket.available:{socket.Available} this.available{available}");
                    Memory<byte> memory = writer.GetMemory(1 << 20);
                    //if (isTimeoutSetForRecieve)
                    //{
                    //    bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cts.Token).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutRecieve)).ConfigureAwait(false);
                    //}
                    //else` 
                    //{
                    //    bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cts.Token).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutRecieveDefault)).ConfigureAwait(false);
                    //}
                    bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cts.Token).ConfigureAwait(false);

                    if (bytesRead > 0)
                    {
#if DEBUG
                        debugger.Push(bytesRead, memory);
#endif
                        ReportTime($"FillPipeAsync ReceiveAsync bytesRead:{bytesRead}  socket.available:{socket.Available} this.available:{available}");
                        countFills++;
                        totalBytesFilled += (uint)bytesRead;
                        Interlocked.Add(ref available, bytesRead);
                        // Tell the PipeWriter how much was read from the Socket
                        writer.Advance(bytesRead);
                        // Make the data available to the
                        //Logger.LogInformation($"{guid}: Filled {bytesRead}. Total:{totalBytesFilled}. Available:{available}");
                        ReportTime($"FillPipeAsync FLUSH Start. readed:{bytesRead}  available: {available}");
                        FlushResult result = await writer.FlushAsync().ConfigureAwait(false);
                        ReportTime($"FillPipeAsync FLUSH End available: {available}");
                        if (readSizeMax < bytesRead) readSizeMax = bytesRead;
                        if (readSizeMin > bytesRead) readSizeMin = bytesRead;
                        ReportTime($"FillPipeAsync End Loop available: {bytesRead}");
                    }
                    else
                    {
                        //ReportTime($"FillPipeAsync Zero Read. Break FillingPipeAsync");
                        ReportTime($"FillPipeAsync Zero Read");
                        //break;
                    }
                    //if (socket.Available == 0)
                    //{
                    //    ReportTime($"FillPipeAsync Socket Zero available. Break FillingPipeAsync");
                    //    break;
                    //}
                }
                catch (TaskCanceledException)
                {
                    ReportTime($"FillPipeAsync Catched TaskCanceledException. SocketAvailable:{socket.Available}. this available: {available}. ReadSizeMin:{readSizeMin} ReadSizeMax:{readSizeMax}");
                    break;
                }
                catch (OperationCanceledException)
                {
                    ReportTime($"FillPipeAsync Catched Operation Canceled Exception. SocketAvailable:{socket.Available}. this available: {available}. ReadSizeMin:{readSizeMin} ReadSizeMax:{readSizeMax}");
                    break;
                }
                catch (SocketException) //System.Net.Sockets.SocketException: 'An existing connection was forcibly closed by the remote host.'
                {
                    ReportTime($"FillPipeAsync Catched Socket exception. SocketAvailable:{socket.Available}. this available: {available}. ReadSizeMin:{readSizeMin} ReadSizeMax:{readSizeMax}");
                    break;
                }
                catch (TimeoutException)
                {
                    ReportTime($"FillPipeAsync Catched Timeout exception. SocketAvailable:{socket.Available}. this available: {available}. ReadSizeMin:{readSizeMin} ReadSizeMax:{readSizeMax}");
                    break;
                }
            }
            ReportTime($"FillPipeAsync End available: {available}. ReadSizeMin:{readSizeMin} ReadSizeMax:{readSizeMax}");
        }

        public virtual async ValueTask<ReadResult> ReadAsync(CancellationToken token = default)
        {
            var reader = Reader ?? throw new NullReferenceException();
            ReportTime($"ReadAsync Start: available:{available}");
            try
            {
                var t1 = reader.ReadAsync(token);
                this.result = await t1.ConfigureAwait(false);
                if (result.IsCompleted) reader.Complete();
                buffer = result.Buffer;
                bufferCurrent = buffer;
                this.position = buffer.Start.GetInteger();
                //Logger.LogInformation($"{guid}: Readed Async:{buffer.Length}");
                ReportTime($"ReadAsync End. Buffer length. {result.Buffer.Length}");
                return result;
            }
            catch (TimeoutException ex)
            {
                ReportTime($"ReadAsync End Timeout Exception. Buffer length. {result.Buffer.Length}");
                throw new TimeoutException($"izhg Read is timeout. isTimeoutSetForRecieve:{isTimeoutSetForRecieve}  " +
                    $"timeoutRecieveDefault:{timeoutRecieveDefault}, timeoutRecieve:{timeoutRecieve}. available:{available}");
            }
        }
        public virtual async ValueTask<ReadResult> ReadAsyncTimeout(CancellationToken token = default)
        {
            ReportTime($"ReadAsync Start: available:{available}");
            try
            {
                if (isTimeoutSetForRecieve)
                {
                    var t1 = reader.ReadAsync(token);
                    this.result = await t1.AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutRecieve)).ConfigureAwait(false);
                }
                else
                {
                    var t1 = reader.ReadAsync(token);
                    this.result = await t1.AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutRecieveDefault)).ConfigureAwait(false);
                }
                buffer = result.Buffer;
                bufferCurrent = buffer;
                this.position = buffer.Start.GetInteger();
                //Logger.LogInformation($"{guid}: Readed Async:{buffer.Length}");
                ReportTime($"ReadAsync End. Buffer length. {result.Buffer.Length}");
                return result;
            }
            catch (TimeoutException ex)
            {
                ReportTime($"ReadAsync End Timeout Exception. Buffer length. {result.Buffer.Length}");
                throw new TimeoutException($"izhg Read is timeout. isTimeoutSetForRecieve:{isTimeoutSetForRecieve}  " +
                    $"timeoutRecieveDefault:{timeoutRecieveDefault}, timeoutRecieve:{timeoutRecieve}. available:{available}");
            }
        }

        public bool CheckConnectIndirectly()
        {
            return writer.UnflushedBytes > 0 || socket.Available > 0 || available > 0;
        }
        public bool CheckDisconnectIndirectly()
        {
            return writer.UnflushedBytes == 0 && socket.Available == 0;
        }
        public bool CheckConnect()
        {
            return socket?.Connected ?? false;
        }

        /// <summary>
        /// Timeouts if specific timeouts is not specified
        /// </summary>
        /// <param name="send"></param>
        /// <param name="recieve"></param>
        public void SetDefaultTimeouts(int send, int recieve)
        {
            this.timeoutSendDefault = send;
            this.timeoutRecieveDefault = recieve;
        }
        public void SetTimeouts(int send, int recieve)
        {
            return;

            isTimeoutSetForRecieve = true;
            isTimeoutSetForSend = true;

            timeoutSend = send;
            timeoutRecieve = recieve;

            socket.SendTimeout = send;
            socket.ReceiveTimeout = recieve;
        }

        private Queue<string> logsLast100 = new Queue<string>(100);
        private List<string> logs = new List<string>();
        private List<string> msgs = new List<string>();
        private List<object> objs = new List<object>();
        private bool isDisposed = true;

        public void ReportTime(string text)
        {
            string log = $"{DateTime.Now.ToString("HH:mm:ss.fffffff")}   {id}:{title} {stopwatch.ElapsedMilliseconds} {text}. Thread:{Thread.CurrentThread.ManagedThreadId}";
            logs.Add(log);
            //Console.WriteLine($"{id}:{title} {stopwatch.ElapsedMilliseconds} {text}");
            logsLast100.Enqueue(log);
            if (logsLast100.Count > 100)
            {
                logsLast100.Dequeue();
            }
            //if (title == "Origin") 
            Console.WriteLine(log);
        }

        public async Task LogsToFile()
        {
            return;

            if (logs.Count > 0)
            {
                string log = $"{id}.{title}.G_{generation}{Environment.NewLine}{logs.Aggregate((x, y) => x + Environment.NewLine + y)}";
                string filename = $"{DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fffff")}_{id}_{title}_{generation}.log";
                File.WriteAllText(filename, log);
                Console.WriteLine("Printed to File");
                logs.Clear();
            }
            if (msgs.Count > 0)
            {
                string text = msgs.Aggregate((x, y) => x + Environment.NewLine + y);
                msgs.Clear();
                string filenameData = $"{DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss.fffff")}_{id}_{title}_{generation}.txt";
                File.WriteAllText(filenameData, text);
            }
            objs.Clear();
        }

        public void PutMsg<T>(T o) where T : ICloneable
        {
            return;

            o = o.DeepClone();

            //if (o is HttpBinary b)
            //{
            //    if (title == "Origin")
            //    {
            //        if (b.type == HttpLibConstants.TYPE_REQUEST)
            //        {
            //            Console.WriteLine($"Not");
            //        }
            //    }
            //    if (title == "Client")
            //    {
            //        if (b.type == HttpLibConstants.TYPE_RESPONSE)
            //        {
            //            Console.WriteLine("WRong");
            //        }
            //    }
            //}
            objs.Add(o);
            msgs.Add(o.ToString());
        }

        public void SetState(EConnectionState state)
        {
            this.state = state;
        }

        public string ToInfoConnectionData()
        {
            return $"ConnectionData. GetType():{GetType().FullName}; host:{Host}; port:{Port}; id:{Id}; action:{Action}; status:{Status}; version:{Version}";
        }
    }
}