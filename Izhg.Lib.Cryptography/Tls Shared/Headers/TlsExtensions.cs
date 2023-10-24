using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Infos;
using IziHardGames.Libs.Cryptography.Tls;

namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct TlsExtensions
    {
        [FieldOffset(0)] private byte major;
        [FieldOffset(1)] private byte minor;
        public int Length => BufferReader.ToUshort(major, minor);
    }

    /// <summary>
    /// <see cref="TlsExtensionInfoReusable"/>
    /// <see cref="TlsExtensionInfo"/>
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct TlsExtension
    {
        [FieldOffset(0)] private byte typeMajor;
        [FieldOffset(1)] private byte typeMinor;
        [FieldOffset(2)] private byte sizeMajor;
        [FieldOffset(3)] private byte sizeMinor;
        public ushort Length => BufferReader.ToUshort(sizeMajor, sizeMinor);
        public ETlsExtensions Type => (ETlsExtensions)TypeUshort;
        public ushort TypeUshort => BufferReader.ToUshort(typeMajor, typeMinor);
    }

    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct ExtDataAlpnProtocolList
    {
        [FieldOffset(0)] private byte lengthMajor;
        [FieldOffset(1)] private byte lengthMinor;

        public ushort Length => BufferReader.ToUshort(lengthMajor, lengthMinor);

        public string ToStringInfo()
        {
            return $"Length:{Length}";
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct ExtDataAlpnProtocolItem
    {
        [FieldOffset(0)] public byte length;
    }

    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct ExtDataClientCertificateUrl
    {
        [FieldOffset(0)] private byte type;
        [FieldOffset(1)] public URLAndHash UrlAndHash;
        public ECertChainType Type => (ECertChainType)type;

        public string ToStringInfo()
        {
            return $"Type:{Type}\tLength:{UrlAndHash.length}";
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    [Header]
    public struct URLAndHash
    {
        [FieldOffset(0)] public byte length;
    }
}
