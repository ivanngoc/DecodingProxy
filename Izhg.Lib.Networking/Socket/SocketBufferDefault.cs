using System;
using System.Buffers;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public class SocketBufferDefault : SocketBuffer
    {
        private byte[] buffer = Array.Empty<byte>();
        private int leftFree;

        public override void Initilize(int size)
        {
            if (size <= 0) throw new ArgumentException("Buffer size must Greater than 0");
            base.Initilize(size);
            var newBuffer = ArrayPool<byte>.Shared.Rent(size);
            buffer = newBuffer;
            leftFree = buffer.Length;
        }
        public override void Dispose()
        {
            base.Dispose();
            if (buffer.Length > 0) ArrayPool<byte>.Shared.Return(buffer);
            buffer = Array.Empty<byte>();
        }

        public override ReadOnlyMemory<byte> PeekAsReadOnlyMemory()
        {
            return new ReadOnlyMemory<byte>(buffer, 0, length);
        }

        public override ReadOnlySequence<byte> PeekAsReadOnlySequence()
        {
            throw new NotImplementedException();
        }

        public override Memory<byte> GetBufferAsMemory()
        {
            return new Memory<byte>(buffer, 0, length);
        }
        /// <summary>
        /// Get Memory To Write Into
        /// </summary>
        /// <returns></returns>
        public override Memory<byte> GetMemory()
        {
            return new Memory<byte>(buffer, length, leftFree);
        }

        public override void Fill(in ReadOnlyMemory<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public override void Fill(in ReadOnlySpan<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public override void Fill(in ReadOnlySequence<byte> bytes)
        {
            throw new NotImplementedException();
        }

        public ValueTask<int> TransferTo(in Memory<byte> mem)
        {
            if (length > mem.Length)
            {
                int size = mem.Length;
                var diff = length - size;
                new Memory<byte>(buffer, 0, size).CopyTo(mem);
                length = diff;
                Array.Copy(buffer, size, buffer, 0, diff) ;  // shift to left
                leftFree += size;
                return ValueTask.FromResult(size);
            }
            else
            {
                new Memory<byte>(buffer, 0, length).CopyTo(mem);
                var result = length;
                length = 0;
                leftFree = buffer.Length;
                return ValueTask.FromResult(result);
            }
        }

        public void Advance(int readed)
        {
            length += readed;
            leftFree -= readed;
        }
    }
}