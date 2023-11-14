using System;
using System.Buffers;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class DataFragment : IDisposable
    {
        private int owner;
        internal Memory<byte> buffer;
        private int length;
        private byte[]? array;

        public DataFragment()
        {

        }
        internal DataFragment(byte[] bytes)
        {
            this.array = bytes;
            buffer = bytes;
            length = bytes.Length;
        }

        internal ReadOnlyMemory<byte> ReadOnly => buffer;
        public int Length => length;

        internal static DataFragment Get(byte[] bytes)
        {
            DataFragment frag = PoolObjectsConcurent<DataFragment>.Shared.Rent();
            frag.SetBuffer(bytes);
            frag.SetLength(bytes.Length);
            return frag;
        }
        internal static DataFragment Get(int available)
        {
            DataFragment frag = PoolObjectsConcurent<DataFragment>.Shared.Rent();
            var array = ArrayPool<byte>.Shared.Rent(available);
            frag.SetBuffer(array);
            return frag;
        }

        private void SetBuffer(byte[] array)
        {
            this.array = array;
            buffer = array;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
        internal void SetLength(int readed)
        {
            this.length = readed;
            buffer = buffer.Slice(0, readed);
        }
    }
}
