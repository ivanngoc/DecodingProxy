using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Async;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.Libs.Streams.Contracts;
using Func = System.Func<System.ReadOnlyMemory<byte>, bool>;
using FuncAsync = IziHardGames.Libs.Async.AsyncOperation<System.Threading.Tasks.Task<bool>>;

namespace IziHardGames.Libs.Streams
{

    /// <summary>
    /// After Each Read Copy Reaed data To Targets
    /// </summary>
    public class StreamDemultiplexer : Stream, IDemultiplexer
    {
        public override bool CanRead { get => innerStream!.CanRead; }
        public override bool CanSeek { get => innerStream!.CanSeek; }
        public override bool CanWrite { get => innerStream!.CanWrite; }
        public override long Length { get => innerStream!.Length; }
        public override long Position { get => innerStream!.Position; set => innerStream!.Position = value; }

        private readonly Dictionary<int, Func> readers = new Dictionary<int, Func>(4);
        private readonly Dictionary<int, Func> writers = new Dictionary<int, Func>(4);
        private readonly Dictionary<int, FuncAsync> readersAsync = new Dictionary<int, FuncAsync>(4);
        private readonly Dictionary<int, FuncAsync> writersAsync = new Dictionary<int, FuncAsync>(4);
        private Stream? innerStream;
        private int counter;
        private int counterWriter;

        public override void Flush()
        {
            innerStream!.Flush();
        }

        public void Initilize(Stream stream)
        {
            innerStream = stream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public int RegistReader(FuncAsync value)
        {
            var key = Interlocked.Increment(ref counter);
            readersAsync.Add(key, value);
            return key;
        }
        public int RegistReader(Func value)
        {
            var key = Interlocked.Increment(ref counter);
            readers.Add(key, value);
            return key;
        }
        public void RegistReaderReverse(int key)
        {
            readers.Remove(key);
        }
        public int RegistWriter(Func action)
        {
            var key = Interlocked.Increment(ref counterWriter);
            writers.Add(key, action);
            return key;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }


        public override int ReadByte()
        {
            throw new System.NotImplementedException();
        }
        public override int Read(Span<byte> buffer)
        {
            throw new System.NotImplementedException();
        }
        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            REPEAT:

            int readed = await innerStream!.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (readed > 0)
            {
                //Console.WriteLine($"[{GetHashCode()}] [{GetType().Name}] Readed:{readed}");
                var slice = buffer.Slice(0, readed);
                await NotifyReadersAsync(slice).ConfigureAwait(false);
                return readed;
            }
            goto REPEAT;
        }

        private async Task NotifyWritersAsync(ReadOnlyMemory<byte> slice)
        {
            var t1 = NotifySubscribersAsync(slice, writersAsync);
            NotifySubscribers(slice, writers);
            await t1.ConfigureAwait(false);
        }
        private async Task NotifyReadersAsync(ReadOnlyMemory<byte> slice)
        {
            var t1 = NotifySubscribersAsync(slice, readersAsync);
            NotifySubscribers(slice, readers);
            await t1.ConfigureAwait(false);
        }

        private static async Task NotifySubscribersAsync(ReadOnlyMemory<byte> buffer, Dictionary<int, FuncAsync> list)
        {
            var result = await Awaiting.WhenResults(list.Values, buffer).ConfigureAwait(false);

            for (int i = 0; i < result.count; i++)
            {
                var asyncOperation = result[i];
                if (asyncOperation.operation is Task<bool> task)
                {
                    if (task.Result)
                    {
                        list.Remove(asyncOperation.id);
                    }
                }
            }
            result.Dispose();
        }




        private static void NotifySubscribers(in ReadOnlyMemory<byte> buffer, Dictionary<int, Func> list)
        {
            Span<int> keysToDelete = stackalloc int[list.Count];
            int count = default;

            foreach (var item in list)
            {
                int key = item.Key;
                var isUnsubscribe = item.Value.Invoke(buffer);
                if (isUnsubscribe)
                {
                    keysToDelete[count] = key;
                    count++;
                }
            }
            for (int i = 0; i < count; i++)
            {
                list.Remove(keysToDelete[i]);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            innerStream.Write(buffer, offset, count);
            var t1 = NotifyWritersAsync(new Memory<byte>(buffer, offset, count));
            t1.Wait();
        }
        public async override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
        public async override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            await innerStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
            await NotifyWritersAsync(buffer).ConfigureAwait(false);
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteByte(byte value)
        {
            throw new System.NotImplementedException();
        }
        public override void Close()
        {
            base.Close();
            this.innerStream = default;
            counter = default;
            counterWriter = default;
        }

      

        private struct Entry
        {
            public Func func;
            public bool isToUnsubscribe;
            public Entry(Func value) : this()
            {
                this.func = value;
            }
            internal void SetUnsubscribe()
            {
                isToUnsubscribe = true;
            }
        }
    }
}
