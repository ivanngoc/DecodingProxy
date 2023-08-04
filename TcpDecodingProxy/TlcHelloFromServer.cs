using System.Runtime.InteropServices;
using IziHardGames.Libs.Buffers.Vectors;

namespace IziHardGames.Proxy.TcpDecoder
{
    [StructLayout(LayoutKind.Explicit)]
    public struct TlcHelloFromServer
    {
        //Record Header
        [FieldOffset(0)] public byte type;
        [FieldOffset(1)] public short protocolVersion;
        [FieldOffset(3)] public short sizeHandshakeMsg;

        // Handshake Header
        [FieldOffset(5)] public byte handshakeMessageType;
        [FieldOffset(6)] public Bytes3 sizeServerHelloData;

        //Server Version
        [FieldOffset(9)] public short serverVersion;

        //Server Random
        [FieldOffset(11)] public Bytes32 randomData;

        //Session ID
        [FieldOffset(43)] public byte sessionId;

        //Cipher Suite
        [FieldOffset(44)] public short cipherSuite;

        //Compression Method
        [FieldOffset(46)] public byte compressionMethod;

        //Extensions Length
        [FieldOffset(47)] public short extensionsLength;

        //Extension - Renegotiation Info
        [FieldOffset(49)] public Bytes5 renegotiationInfo;
    }


    [StructLayout(LayoutKind.Explicit, Size = 5)]
    public struct Bytes5
    {
        [FieldOffset(0)] public byte b0;
        [FieldOffset(1)] public byte b1;
        [FieldOffset(2)] public byte b2;
        [FieldOffset(3)] public byte b3;
        [FieldOffset(4)] public byte b4;
    }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Bytes32
    {
        [FieldOffset(0)] public int i0;
        [FieldOffset(4)] public int i1;
        [FieldOffset(8)] public int i2;
        [FieldOffset(12)] public int i3;

        [FieldOffset(16)] public int i4;
        [FieldOffset(20)] public int i5;
        [FieldOffset(24)] public int i6;
        [FieldOffset(28)] public int i7;
    }
}