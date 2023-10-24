using System;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Buffers.Abstracts
{
    public abstract class AdapterForRead : IDisposable
    {
        protected SourceAdapter? source;
        public virtual void SetSource(SourceAdapter source)
        {
            this.source = source;
        }
        public virtual void Dispose()
        {
            source = default;
        }
        public abstract int Read(byte[] buffer, int offset, int count);
        public abstract int Read(in Span<byte> buffer);
        public abstract ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default);
    }
}
