using System;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Buffers.Sources
{
    public class SourceAdapterForMemory : SourceAdapter, IPoolBind<SourceAdapterForMemory>
    {
        public Memory<byte> source;
        private IPoolReturn<SourceAdapterForMemory>? pool;

        public override bool CanRead { get ; set; }
        public override bool CanWrite { get; set; }
        public override bool CanSeek { get => true; set => throw new NotSupportedException(); }
        public override long Length { get => source.Length; set => throw new NotImplementedException(); }
        public override long Position { get; set; }

        public static SourceAdapterForMemory GetOrCreate()
        {
            var pool = PoolObjectsConcurent<SourceAdapterForMemory>.Shared;
            SourceAdapterForMemory item = pool.Rent();
            item.BindToPool(pool);
            return item;
        }

        public void BindToPool(IPoolReturn<SourceAdapterForMemory> pool)
        {
            this.pool = pool;
        }

        public override void Dispose()
        {
            base.Dispose();
            pool!.Return(this);
            pool = default;
        }
    }
}
