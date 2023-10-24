using System;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Streams
{
    public class SourceAdapterForReadOnlyMemory : SourceAdapter, IPoolBind<SourceAdapterForReadOnlyMemory>
    {
        public ReadOnlyMemory<byte> source;
        private IPoolReturn<SourceAdapterForReadOnlyMemory>? pool;

        public override bool CanRead { get => true; set => throw new System.NotSupportedException(); }
        public override bool CanSeek { get => true; }
        public override bool CanWrite { get => false; }
        public override long Length { get => source.Length; }
        public override long Position { get; set; }

        internal static SourceAdapterForReadOnlyMemory GetOrCreate()
        {
            var pool = PoolObjectsConcurent<SourceAdapterForReadOnlyMemory>.Shared;
            SourceAdapterForReadOnlyMemory item = pool.Rent();
            item.BindToPool(pool);
            return item;
        }

        public void BindToPool(IPoolReturn<SourceAdapterForReadOnlyMemory> pool)
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
