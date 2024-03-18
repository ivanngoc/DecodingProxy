using System;
using System.Buffers;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class DataFrame : IDisposable
    {
        private byte[]? bytes;
        private Memory<byte> memory;
        private int length;
        public EFrameType type;
        public int Length => length;
        public ReadOnlyMemory<byte> ReadOnly => memory;
        public Memory<byte> Write => memory;


        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static DataFrame Allocate(int length)
        {
            var frame = IziPool.GetConcurrent<DataFrame>();
            frame.EnsureCapacity(length);
            return frame;
        }

        private void EnsureCapacity(int length)
        {
            if (bytes != default && bytes.Length > 0)
            {
                if (bytes.Length >= length) return;
                var newBuf = ArrayPool<byte>.Shared.Rent(length);
                Array.Copy(bytes, 0, newBuf, 0, length);
                ArrayPool<byte>.Shared.Return(bytes);
                bytes = newBuf;
                memory = bytes.AsMemory().Slice(0, length);
            }
            else
            {
                bytes = ArrayPool<byte>.Shared.Rent(length);
            }
        }

        internal void CopyAppend(ReadOnlyMemory<byte> data)
        {
            int start = this.length;
            this.length += data.Length;
            this.memory = bytes.AsMemory().Slice(0, length);
            var dest = this.memory.Slice(start, data.Length);
            data.CopyTo(dest);
        }

        internal void CopySet(ReadOnlyMemory<byte> data)
        {
            memory = bytes.AsMemory().Slice(0, data.Length);
            data.CopyTo(memory);
            length = data.Length;
        }
    }
}
