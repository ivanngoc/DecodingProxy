using System;

namespace IziHardGames.Libs.Buffers.Abstracts
{
    public abstract class SourceAdapter : IDisposable
    {
        public abstract bool CanRead { get; set; }
        public abstract bool CanSeek { get; set; }
        public abstract bool CanWrite { get; set; }
        public abstract long Length { get; set; }
        public abstract long Position { get; set; }

        public virtual void Dispose()
        {

        }
    }
}
