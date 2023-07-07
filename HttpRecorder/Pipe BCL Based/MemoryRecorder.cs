using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Consuming;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO.Pipelines;
using System.Text;
using System.Text.Unicode;

namespace IziHardGames.Proxy.Recoreder.MemoryBased
{
    /// <summary>
    /// Based on <see cref="ReadOnlySequence{T}"/>
    /// </summary>
    public class MemoryRecorder
    {
        public ConcurrentDictionary<int, MemoryRecoreder> records = new ConcurrentDictionary<int, MemoryRecoreder>();

        public virtual void Push(DataSource dataSource, ReadOnlySequence<byte> data)
        {
            var existed = records.GetOrAdd(dataSource.id, (x) => RegistNewSource(dataSource));
            existed.Write(data);
        }
        public MemoryRecoreder RegistNewSource(DataSource dataSource)
        {
            MemoryRecoreder item = PoolObjectsConcurent<MemoryRecoreder>.Shared.Rent();
            item.Bind(dataSource);
            return item;
        }
    }

    public class RequestMemoryRecorder : MemoryRecorder
    {

    }
    public class ResponseMemoryRecorder : MemoryRecorder
    {

    }

    public class MemoryRecoreder : IDisposable
    {
        private DataSource source;
        public readonly Pipe pipe;
        public readonly PipeReader pipeReader;
        public readonly PipeWriter pipeWriter;

        public MemoryRecoreder()
        {
            pipe = new Pipe();
            pipeReader = pipe.Reader;
            pipeWriter = pipe.Writer;
        }

        internal void Bind(DataSource dataSource)
        {
            this.source = dataSource;
        }
        internal void Write(ReadOnlySequence<byte> data)
        {
            var mem = pipeWriter.GetSpan((int)data.Length);
            data.CopyTo(mem);
            pipeWriter.Advance(mem.Length);
        }

        public async Task PrintToFile()
        {
            var result = await pipeReader.ReadAsync();
            var buffer = result.Buffer;
            string path = $"logs{Path.DirectorySeparatorChar}{source.id}.log";

            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read, (int)buffer.Length, FileOptions.Asynchronous))
            {
                foreach (var segment in buffer)
                {
                    await fs.WriteAsync(segment);
                }
            }
        }
        public async Task PrintToConsole()
        {
            var result = await pipeReader.ReadAsync();
            var buffer = result.Buffer;

            foreach (var segment in buffer)
            {
                Console.WriteLine(Encoding.UTF8.GetString(segment.Span));
            }
        }

        public void Dispose()
        {            
            pipeWriter.Complete();
            pipeReader.Complete();  
            source = default;
            pipe.Reset();
        }
    }
}