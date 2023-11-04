using System;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Buffers.Abstracts
{
    public abstract class AdapterForWrite : IDisposable
    {
        private SourceAdapter? source;
        public virtual void Dispose()
        {
            source = default;
        }

        public virtual void SetSource(SourceAdapter source)
        {
            this.source = source;
            source.CanWrite = true;
        }
        public abstract Task WriteAsync(ReadOnlyMemory<byte> buffer);
    }
}
