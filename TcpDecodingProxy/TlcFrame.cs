using System.Runtime.InteropServices;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TlcFrame
    {
        public static void FromStream(Stream stream)
        {

        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct TlcHelloFromClient
    {
        // Record Header
        [FieldOffset(0)] public byte type;
        [FieldOffset(1)] public short protocolVersion;
        [FieldOffset(3)] public short msgSize;

        // handshakeHeader
        [FieldOffset(5)] public byte handshakeMessageType;
        [FieldOffset(6)] public Bytes3 sizeClientHello;

        // Client Version
        [FieldOffset(9)] public short clientVersion;
    }

    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct Bytes3
    {
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
    }
}