using System;
using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Buffers.Vectors;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#appendix-A.4.1
    /// </summary>

    [StructLayout(LayoutKind.Explicit, Size = ConstantsForTls.SIZE_RANDOM)]
    [Header]
    public struct TlsRandom
    {
        /// <summary>
        /// Not Tested for Correct
        /// </summary>
        [FieldOffset(0)] private uint gmt_unix_time;
        [FieldOffset(4)] public Bytes28 random;
        /// <summary>
        /// Not Tested for Correct
        /// </summary>
        public uint Seconds => BufferReader.ReverseEndians(gmt_unix_time);
        /// <summary>
        /// Not Tested for Correct
        /// </summary>
        public DateTime DateTime => new DateTime().AddSeconds(Seconds);
    }
}
