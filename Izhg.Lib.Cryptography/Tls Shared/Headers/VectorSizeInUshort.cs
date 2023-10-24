using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    [Header]
    public struct VectorSizeInUshort
    {
        [FieldOffset(0)] private byte major;
        [FieldOffset(1)] private byte minor;

        public ushort Length => BufferReader.ToUshort(major, minor);
    }
}
