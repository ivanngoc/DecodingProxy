using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Libs.Buffers.Vectors;

namespace IziHardGames.Libs.ForHttp20
{
    [StructLayout(LayoutKind.Explicit, Size = 9)]
    public struct HttpFrame
    {
        [FieldOffset(0)] public Bytes24 length;
        [FieldOffset(3)] public byte type;
        [FieldOffset(4)] public byte flags;
        [FieldOffset(5)] public ulong streamIdentifier;
        public static HttpFrame Settings => new HttpFrame() { type = 0x04 };

        public unsafe void WriteThisTo(NetworkStream stream)
        {
            var copy = this;
            Span<HttpFrame> span = new Span<HttpFrame>(&copy, 1);
            Span<byte> bytes = MemoryMarshal.Cast<HttpFrame, byte>(span);
            stream.Write(bytes);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 6)]
    public struct Setting
    {
        [FieldOffset(0)] public ushort identifier;
        [FieldOffset(2)] public int value;
    }

    public enum EFrameType
    {
        None,
        Settings = 0x04,
    }
}
