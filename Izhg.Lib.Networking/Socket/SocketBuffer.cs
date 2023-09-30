using System;
using System.Buffers;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public abstract class SocketBuffer : IDisposable
    {
        private bool isDisposed = true;
        protected int length;
        public int Length => length;

        public virtual void Initilize(int size)
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed befire reuse");
            isDisposed = false;
        }
        public virtual void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException($"Object is Already Disposed");
            isDisposed = true;
        }

        public abstract ReadOnlyMemory<byte> PeekAsReadOnlyMemory();
        public abstract ReadOnlySequence<byte> PeekAsReadOnlySequence();
        public abstract Memory<byte> GetBufferAsMemory();
        public abstract Memory<byte> GetMemory();

        public abstract void Fill(in ReadOnlyMemory<byte> bytes);
        public abstract void Fill(in ReadOnlySpan<byte> bytes);
        public abstract void Fill(in ReadOnlySequence<byte> bytes);
    }
}