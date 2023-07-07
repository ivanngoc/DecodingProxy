using IziHardGames.Libs.NonEngine.Memory;
using System.Buffers;

namespace IziHardGames.Proxy.Datas
{
    public class Block : IDisposable
    {
        public Memory<byte> MemoryData => new Memory<byte>(data, 0, length);
        public Memory<byte> MemoryRaw => new Memory<byte>(data);
        public byte[] Data => data;
        private byte[]? data;
        public int length;

        private uint idSource;
        private int type;
        private int countConsumers;
        private int sequenceNumber;

        public uint IdSource => idSource;
        public int SequenceNumber => sequenceNumber;


        public static Block Create(int size, uint idSource, int type, int sequenceNumber)
        {
            return PoolObjects<Block>.Shared.Rent().InitToReuse(size, idSource, type, sequenceNumber);
        }

        private Block InitToReuse(int prealocSize, uint id, int type, int sequenceNumber)
        {
            var rent = ArrayPool<byte>.Shared.Rent(prealocSize);
            data = rent;

            this.idSource = id;
            this.type = type;
            this.sequenceNumber = sequenceNumber;

            return this;
        }

        public void TryDispose()
        {
            if (countConsumers > 0)
            {
                return;
            }
            Dispose();
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(data!);
            data = default;

            idSource = default;
            sequenceNumber = 0;
            length = default;
        }

        public void SetLength(int size)
        {
            this.length = size;
        }

        public void Use()
        {
            int newVal = Interlocked.Increment(ref countConsumers);
        }
        public void Unuse()
        {
            int newVal = Interlocked.Decrement(ref countConsumers);
        }

        internal void SetSequnceNumber(int sequenceNumber)
        {
            this.sequenceNumber = sequenceNumber;
        }

        internal void Release()
        {
            throw new NotImplementedException();
        }
    }
}