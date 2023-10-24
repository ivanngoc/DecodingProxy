using System;

namespace IziHardGames.Libs.Buffers.Abstracts
{
    public abstract class SourceAdapter : IDisposable
    {
        public abstract bool CanRead { get; set; }
        public abstract bool CanSeek { get; }
        public abstract bool CanWrite { get; }
        public abstract long Length { get; }
        public abstract long Position { get; set; }

        public virtual void Dispose()
        {

        }
    }
}
