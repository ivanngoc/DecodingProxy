using System;
using System.Runtime.InteropServices;
using IziHardGames.Libs.Cryptography.Tls12;
using static IziHardGames.Libs.Cryptography.Tls12.TlsEnums;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// <see cref="ETlsProtocol"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct ChangeCipherSpec
    {
        [FieldOffset(0)] private byte type;
        public EChangeCipherSpec Type => (EChangeCipherSpec)type;
        internal string ToStringInfo()
        {
            return Type.ToString();
        }
    }
}
