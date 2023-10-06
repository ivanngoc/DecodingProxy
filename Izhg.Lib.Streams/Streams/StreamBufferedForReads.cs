using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Buffers.Attributes;
using IziHardGames.Libs.Streams.Contracts;

namespace IziHardGames.Libs.Streams
{
    public class StreamBufferedForReads : Stream, IStream
    {
        public override bool CanRead { get => innerStream.CanRead; }
        public override bool CanSeek { get => true; }
        public override bool CanWrite { get => innerStream.CanWrite; }
        public override long Length { get => length; }
        public override long Position { get; set; }

        private bool isDisposed = true;
        private int length;
        private int segmentSize;
        private int flagReadToBufferIsRunning;

        private Stream innerStream;
        private CancellationTokenSource? cts;
        private Task? fillingTask;
        private ConcurrentQueue<BufferSegment> buffers = new ConcurrentQueue<BufferSegment>();

        public void Initilize(Stream stream, int segmentSize = ((1 << 10) * 32))
        {
            if (!isDisposed) throw new ObjectDisposedException("Object must be disposed before use");
            isDisposed = false;
            this.innerStream = stream;
            this.segmentSize = segmentSize;
        }

        public override void Flush()
        {
            innerStream!.Flush();
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



        public Task StartFillingBuffer()
        {
            int flag = Interlocked.CompareExchange(ref flagReadToBufferIsRunning, 1, 0);
            if (flag != 0)
            {
                throw new InvalidOperationException("Filling buffer is already runned");
            }
            cts = new CancellationTokenSource();
            var t1 = ReadToBufferAsync(cts.Token);
            fillingTask = t1;
            return t1;
        }
        public async Task StopFillingBuffer()
        {
            var flag = Interlocked.CompareExchange(ref flagReadToBufferIsRunning, 0, 1);
            if (flag != 1) throw new System.InvalidOperationException("Trying to stop process that hasn't star yet");
            cts!.Cancel();
            try
            {
                await fillingTask!.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {

            }
            fillingTask = default;
            cts = default;
        }
        private async Task ReadToBufferAsync(CancellationToken ct = default)
        {
            while (!ct.IsCancellationRequested)
            {
                await ReadToBufferUntilZeroAsync(ct).ConfigureAwait(false);
            }
        }

        [ZeroReadBlock]
        private async Task ReadToBufferOnceAsync(CancellationToken token = default)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(segmentSize);
            while (true)
            {
                // if innerStream blocks zero-read than this method will also stuck
                var readed = await innerStream!.ReadAsync(buffer.AsMemory(), token).ConfigureAwait(false);
                if (readed > 0)
                {
                    Interlocked.Add(ref length, readed);
                    buffers.Enqueue(new BufferSegment(buffer, readed));
                    return;
                }
            }
        }

        [ReadUntilZero]
        private async Task ReadToBufferUntilZeroAsync(CancellationToken token = default)
        {
            while (true)
            {
                byte[] buffer = ArrayPool<byte>.Shared.Rent(segmentSize);
                // if innerStream blocks zero-read than this method will also stuck
                var readed = await innerStream!.ReadAsync(buffer.AsMemory(), token).ConfigureAwait(false);
                if (readed > 0)
                {
                    Interlocked.Add(ref length, readed);
                    buffers.Enqueue(new BufferSegment(buffer, readed));
                }
                else
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                    break;
                }
            }
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        [ZeroReadBlock]
        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
        {
            int left = buffer.Length;
            int offset = 0;

            while (left > 0 && offset == 0)
            {
                REPEAT:
                if (buffers.TryPeek(out var bufferSegment))
                {
                    if (bufferSegment.TransferTo(buffer.Slice(offset, left)))
                    {
                        //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Readed From Buffer:{bufferSegment.copied} copied");
                        offset += bufferSegment.copied;
                        left -= bufferSegment.copied;
                        Interlocked.Add(ref this.length, -bufferSegment.copied);
                        bufferSegment.Dispose();

                        if (buffers.TryDequeue(out BufferSegment segment))
                        {
                            continue;
                        }
                        else
                        {
                            throw new InvalidOperationException("Unexpected state");
                        }
                    }
                    else
                    {
                        //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Readed From Buffer:{offset}");
                        Interlocked.Add(ref this.length, -bufferSegment.copied);
                        offset += bufferSegment.copied;
                        return offset;
                    }
                }
                // blocking zero read. If that method called at least 1 succeded read must be perfomed
                else
                {
                    int flag = Interlocked.CompareExchange(ref flagReadToBufferIsRunning, flagReadToBufferIsRunning, flagReadToBufferIsRunning);
                    if (flag == 0)
                    {
                        await ReadToBufferOnceAsync(ct).ConfigureAwait(false);
                        if (!buffers.IsEmpty) goto REPEAT;
                    }
                }
            }
            //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Readed From Buffer:{offset}. Break while");
            return offset;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await innerStream!.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
        }
        public async override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await innerStream!.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        public override void WriteByte(byte value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
            if (isDisposed) throw new ObjectDisposedException($"Object must be disposed before use");
            isDisposed = true;
            length = default;
            if (buffers.Count > 0) throw new InvalidOperationException($"All Reads from buffer must be completed");
            innerStream = default;
            if (fillingTask != null) throw new TaskCanceledException("Task Must be completed before dispose");
            cts = default;
            fillingTask = default;
        }


    }

    public struct BufferSegment : IDisposable
    {
        public byte[] buffer;
        public int length;
        public int offset;
        public int copied;

        public BufferSegment(byte[] buffer, int length) : this()
        {
            this.buffer = buffer;
            this.length = length;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        internal ReadOnlyMemory<byte> GetReadOnlyMemory()
        {
            return new ReadOnlyMemory<byte>(buffer, offset, length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="memory"></param>
        /// <param name="maxToCopy"></param>
        /// <returns>
        /// <see langword="true"/> - No Data left in this segment after copy<br/>
        /// <see langword="false"/> - There is still Data remaining
        /// </returns>
        internal bool TransferTo(in Memory<byte> memory)
        {
            if (memory.Length > length)
            {
                int toCopy = length;
                new Memory<byte>(buffer, offset, toCopy).CopyTo(memory);
                copied = toCopy;
                return true;
            }
            else
            {
                int toCopy = memory.Length;
                new Memory<byte>(buffer, offset, toCopy).CopyTo(memory);
                copied = toCopy;
                Take(toCopy);
                return false;
            }
        }
        internal void Take(int taken)
        {
            offset += taken;
            length -= taken;
        }
    }
}
