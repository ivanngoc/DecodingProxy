using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct CipherSuite
    {
        [FieldOffset(0)] private byte major;
        [FieldOffset(1)] private byte minor;
        public int Length => BufferReader.ToUshort(major, minor);
    }
}
