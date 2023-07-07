using IziHardGames.Libs.Streaming;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace IziHardGames.Libs.Networking.Pipelines
{
    /*
     Схема работы: FillPipe => ReadAsync => Parse => Apply Controls => Analyz Continuation => Break/Continue 
     */
    public class PipedSocket : PipedStream, IDisposable
    {
        public Stopwatch stopwatch = new Stopwatch();
        protected string title;
        protected int id;
        protected int generation;

        protected Socket socket;
        protected IPEndPoint ipEndPoint;
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

        //public readonly PipeWriter writer;
        //public readonly PipeReader reader;
        //public readonly Pipe pipe;

        public PipedSocket() : base()
        {
            //pipe = new Pipe();
            //writer = pipe.Writer;
            //reader = pipe.Reader;
        }

        public override void Close()
        {
            ReportTime($"Socket closed");
            if (socket.Connected) socket.Disconnect(false);
            socket.Close();
            socket.Dispose();
            base.Close();
            pipe.Reset();
            ipEndPoint = default;
            socket = default;

            position = default;
            available = default;
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
            pipe.Reset();
        }
        public void BindTitle(string title)
        {
            this.title = title;
        }
        protected void BindSocket(Socket socket)
        {
            this.socket = socket;
        }
        public void Init()
        {
            generation++;
            stopwatch.Start();
            id = GetHashCode();
            SetDefaultTimeouts(60000, 6000);
        }
        public async Task ConnectAsync(string host, int port)
        {
            ReportTime($"ConnectAsync Start");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.DualMode = true;
            //socket.ExclusiveAddressUse = true;
            socket.ReceiveBufferSize = (1 << 10) * 32;
            socket.SendBufferSize = (1 << 10) * 32;
            await socket.ConnectAsync(host, port).ConfigureAwait(false);
            ReportTime($"ConnectAsync End");
        }
        public void ReportConsume(SequencePosition position)
        {
            bufferCurrent = bufferCurrent.Slice(position);
            reader.AdvanceTo(position, position);
            int newPos = position.GetInteger();
            int consumed = newPos - this.position;
            ReportConsume(consumed);
            this.position = newPos;
            //Logger.LogInformation($"{guid}: Consumed:{consumed}. Total:{totalBytesConsumed}. Available:{available}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReportConsume(long consumed)
        {
            ReportConsume((int)consumed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReportConsume(int consumed)
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
        public async Task FillPipeAsync(CancellationTokenSource cts)
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
                    //else
                    //{
                    //    bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cts.Token).AsTask().WaitAsync(TimeSpan.FromMilliseconds(timeoutRecieveDefault)).ConfigureAwait(false);
                    //}
                    bytesRead = await socket.ReceiveAsync(memory, SocketFlags.None, cts.Token).ConfigureAwait(false);

                    if (bytesRead > 0)
                    {
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
                        writer.Complete();
                        ReportTime($"FillPipeAsync Zero Read. Break FillingPipeAsync");
                        break;
                    }
                    //if (socket.Available == 0)
                    //{
                    //    ReportTime($"FillPipeAsync Socket Zero available. Break FillingPipeAsync");
                    //    break;
                    //}
                }
                catch (SocketException)
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

        public async ValueTask<ReadResult> ReadAsync(CancellationToken token = default)
        {
            ReportTime($"ReadAsync Start: available:{available}");
            try
            {
                var t1 = reader.ReadAsync(token);
                this.result = await t1;
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
        public async ValueTask<ReadResult> ReadAsyncTimeout(CancellationToken token = default)
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

        private List<string> logs = new List<string>();
        private List<string> msgs = new List<string>();
        public void ReportTime(string text)
        {
            logs.Add($"{id}:{title} {stopwatch.ElapsedMilliseconds} {text}. Thread:{Thread.CurrentThread.ManagedThreadId}");
            //Console.WriteLine($"{id}:{title} {stopwatch.ElapsedMilliseconds} {text}");
        }

        public async Task LogsToFile()
        {
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
        }

        internal void PutMsg(string v)
        {
            msgs.Add(v);
        }
    }
}