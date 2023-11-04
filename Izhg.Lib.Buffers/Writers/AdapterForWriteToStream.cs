using System;
using System.IO;
using System.Threading.Tasks;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.Buffers.Sources;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Buffers.Writers
{
    public class AdapterForWriteToStream : AdapterForWrite
    {
        private Stream sourceStream;
        public static AdapterForWriteToStream GetOrCreate()
        {
            return PoolObjectsConcurent<AdapterForWriteToStream>.Shared.Rent();
        }
        public override void SetSource(SourceAdapter source)
        {
            base.SetSource(source);
            this.sourceStream = (source as SourceAdapterForStream)!.source;
        }
        public async override Task WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            await sourceStream.WriteAsync(buffer).ConfigureAwait(false); 
        }
    }
}
