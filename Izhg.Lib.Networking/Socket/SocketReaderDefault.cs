using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public class SocketModifierReaderDefault : SocketWrapModifier
    {
        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
            var pool = PoolObjectsConcurent<SocketReaderDefault>.Shared;
            SocketReaderDefault reader = pool.Rent();
            reader.Initilize(wrap);
        }

        public override void InitilizeReverse()
        {
            base.InitilizeReverse();
        }
    }

    public class SocketReaderDefault : SocketReader
    {
        internal static SocketReader Rent()
        {
            throw new NotImplementedException();
        }
        public override void Initilize(SocketWrap wrap)
        {
            base.Initilize(wrap);
        }

        public async override ValueTask<int> TransferToAsync(Memory<byte> mem, CancellationToken ct = default)
        {
            return await source!.TransferToAsync(mem, ct);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int TransferTo(byte[] array, int offset, int length)
        {
            return source.TransferTo(array, offset, length);
        }
    }
}