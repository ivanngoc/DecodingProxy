using System;
using System.Buffers;
using IziHardGames.Pools.Abstractions.NetStd21;
using IziHardGames.NodeProxies.Nodes.SOCKS5;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.NodeProxies.Nodes
{
    internal class DataFragment : IDisposable
    {
        private Node owner;
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

        internal static void Destroy(ref DataFragment? dataFragment)
        {
            dataFragment!.Dispose();
            IziPool.ReturnConcurrent(dataFragment);
            dataFragment = null;
        }

        internal static DataFragment Get(byte[] bytes)
        {
            DataFragment frag = IziPool.GetConcurrent<DataFragment>();
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

        internal void SetOwner(Node node)
        {
            this.owner = node;
        }

        internal void From(DataFrame frame)
        {
            throw new NotImplementedException();
        }
    }
}
