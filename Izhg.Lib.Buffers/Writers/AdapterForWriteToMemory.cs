using System;
using System.Threading.Tasks;
using IziHardGames.Libs.Buffers.Abstracts;
using IziHardGames.Libs.Buffers.Sources;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Buffers.Writers
{
    public class AdapterForWriteToMemory : AdapterForWrite
    {
        private Memory<byte> sourceMem;
        private int offset;
        private int lengthLeft;
        public static AdapterForWriteToMemory GetOrCreate()
        {
            return PoolObjectsConcurent<AdapterForWriteToMemory>.Shared.Rent();
        }
        public override void SetSource(SourceAdapter source)
        {
            base.SetSource(source);
            this.sourceMem = (source as SourceAdapterForMemory)!.source;
            lengthLeft = sourceMem.Length;
        }
        public override Task WriteAsync(ReadOnlyMemory<byte> buffer)
        {
            int lengthToWrite = lengthLeft > buffer.Length ? buffer.Length : lengthLeft;
            var toWrite = buffer.Slice(0, lengthToWrite);
            toWrite.CopyTo(sourceMem.Slice(offset, lengthToWrite));
            return Task.CompletedTask;
        }
    }
}
