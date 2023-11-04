using System.IO;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Buffers.Sources
{
    public class SourceAdapterForStream : SourceAdapter, IPoolBind<SourceAdapterForStream>
    {
        public Stream source;
        private IPoolReturn<SourceAdapterForStream>? pool;
        public override bool CanRead { get; set; }
        public override bool CanSeek { get; set; }
        public override bool CanWrite { get; set; }
        public override long Length { get; set; }
        public override long Position { get; set; }

        public static SourceAdapterForStream GetOrCreate()
        {
            var pool = PoolObjectsConcurent<SourceAdapterForStream>.Shared;
            SourceAdapterForStream item = pool.Rent();
            item.BindToPool(pool);
            return item;
        }

        public void BindToPool(IPoolReturn<SourceAdapterForStream> pool)
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