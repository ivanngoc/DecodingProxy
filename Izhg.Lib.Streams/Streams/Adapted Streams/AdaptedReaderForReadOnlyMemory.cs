using System;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Core.Buffers;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Streams
{
    public class AdaptedReaderForReadOnlyMemory : AdapterForRead, IPoolBind<AdaptedReaderForReadOnlyMemory>
    {
        private IPoolReturn<AdaptedReaderForReadOnlyMemory>? pool;
        private SourceAdapterForReadOnlyMemory? sourceAdapter;
        protected int offset;
        protected int lengthLeft;

        public override void SetSource(SourceAdapter source)
        {
            base.SetSource(source);
            this.sourceAdapter = (source as SourceAdapterForReadOnlyMemory) ?? throw new NullReferenceException($"Expected typeof:{typeof(SourceAdapterForReadOnlyMemory).FullName} But Recived:{source.GetType().FullName}");
        }

        internal static AdapterForRead GetOrCreate()
        {
            var pool = PoolObjectsConcurent<AdaptedReaderForReadOnlyMemory>.Shared;
            AdaptedReaderForReadOnlyMemory item = pool.Rent();
            item.BindToPool(pool);
            return item;
        }

        #region Reads
        public override int Read(byte[] buffer, int offset, int count)
        {
            return CopyUtil.Copy(sourceAdapter!.source, this.offset, lengthLeft, buffer, offset, count);
        }
        public override int Read(in Span<byte> buffer)
        {
            return CopyUtil.Copy(sourceAdapter!.source, this.offset, lengthLeft, in buffer);
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult<int>(CopyUtil.Copy(sourceAdapter!.source, this.offset, lengthLeft, buffer.Span));
        }
        #endregion

        public void BindToPool(IPoolReturn<AdaptedReaderForReadOnlyMemory> pool)
        {
            this.pool = pool;
        }
        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;
            source = default;
        }
    }
}
