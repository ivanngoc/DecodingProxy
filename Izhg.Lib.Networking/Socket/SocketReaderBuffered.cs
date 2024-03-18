using System;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public class SocketReaderBuffered<T> : SocketReader, IPoolBind<SocketReaderBuffered<T>>
        where T : SocketBuffer
    {
        private readonly SocketBufferDefault buffer = new SocketBufferDefault();
        public SocketBufferDefault Buffer => buffer;
        public void BindToPool(IPoolReturn<SocketReaderBuffered<T>> pool)
        {
            throw new NotImplementedException();
        }
        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            buffer.Initilize((1 << 20) * 4);
        }
        public override void Dispose()
        {
            base.Dispose();
            buffer.Dispose();
        }
        public async override ValueTask<int> TransferToAsync(Memory<byte> mem, CancellationToken ct = default)
        {
            if (buffer.Length == 0)
            {
                await ReadToBufferAsync(ct).ConfigureAwait(false);
            }
            var result = buffer.TransferTo(in mem);
            return result.Result;
        }
        /// <summary>
        /// Copy incoming data without affecting source
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task CopyToBuffer(CancellationToken ct = default)
        {
            throw new System.NotImplementedException();
        }
        /// <summary>
        /// Read and take out data from source
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task ReadToBufferAsync(CancellationToken ct = default)
        {
            var mem = buffer.GetMemory();
            var readed = await source!.TransferToAsync(mem, ct).ConfigureAwait(false);
            buffer.Advance(readed);
            foreach (var interceptor in interceptorsIn)
            {
                var status = interceptor.Intercept(mem.Slice(0, readed));
            }
        }

        public override int TransferTo(byte[] array, int offset, int length)
        {
            var readed = source!.TransferTo(array, offset, length);
            buffer.Advance(readed);
            return readed;
        }
    }
}