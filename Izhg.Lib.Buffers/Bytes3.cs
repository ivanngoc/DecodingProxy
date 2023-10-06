using System.Runtime.InteropServices;
using IziHardGames.Libs.Binary.Readers;

namespace IziHardGames.Libs.Buffers.Vectors
{

    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public struct TestOverlapSize
    {
        [FieldOffset(0)] public int val0;
        [FieldOffset(1)] public int val1;
        [FieldOffset(2)] public int val2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 3)]
    public readonly struct Bytes3
    {
        [FieldOffset(0)] public readonly byte byte0;
        [FieldOffset(1)] public readonly byte byte1;
        [FieldOffset(2)] public readonly byte byte2;

        public static implicit operator int(Bytes3 b) => BufferReader.ToInt32(b.byte0, b.byte1, b.byte2);
    }

    [StructLayout(LayoutKind.Explicit, Size = 9)]
    public struct Bytes9
    {
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
        [FieldOffset(4)] public byte byte4;
        [FieldOffset(5)] public byte byte5;
        [FieldOffset(6)] public byte byte6;
        [FieldOffset(7)] public byte byte7;
        [FieldOffset(8)] public byte byte8;
    }


    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public struct Bytes24
    {
        [FieldOffset(0)] public byte byte0;
        [FieldOffset(1)] public byte byte1;
        [FieldOffset(2)] public byte byte2;
        [FieldOffset(3)] public byte byte3;
        [FieldOffset(4)] public byte byte4;
        [FieldOffset(5)] public byte byte5;
        [FieldOffset(6)] public byte byte6;
        [FieldOffset(7)] public byte byte7;

        [FieldOffset(8)] public byte byte8;
        [FieldOffset(9)] public byte byte9;
        [FieldOffset(10)] public byte byte10;
        [FieldOffset(11)] public byte byte11;
        [FieldOffset(12)] public byte byte12;
        [FieldOffset(13)] public byte byte13;
        [FieldOffset(14)] public byte byte14;
        [FieldOffset(15)] public byte byte15;

        [FieldOffset(16)] public byte byte16;
        [FieldOffset(17)] public byte byte17;
        [FieldOffset(18)] public byte byte18;
        [FieldOffset(19)] public byte byte19;
        [FieldOffset(20)] public byte byte20;
        [FieldOffset(21)] public byte byte21;
        [FieldOffset(22)] public byte byte22;
        [FieldOffset(23)] public byte byte23;

        public static implicit operator int(Bytes24 b) => BufferReader.ToInt32(b.byte0, b.byte1, b.byte2);
    }

    [StructLayout(LayoutKind.Explicit, Size = 31)]
    public struct Bytes31
    {
        [FieldOffset(0)] public int int0;
        [FieldOffset(4)] public int int1;
        [FieldOffset(8)] public int int2;
        [FieldOffset(12)] public int int3;
        [FieldOffset(16)] public int int4;
        [FieldOffset(20)] public int int5;
        [FieldOffset(24)] public int int6;
        [FieldOffset(28)] public byte byte0;
        [FieldOffset(29)] public byte byte1;
        [FieldOffset(30)] public byte byte2;
    }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public struct Bytes32
    {
        [FieldOffset(0)] public int int0;
        [FieldOffset(4)] public int int1;
        [FieldOffset(8)] public int int2;
        [FieldOffset(12)] public int int3;
        [FieldOffset(16)] public int int4;
        [FieldOffset(20)] public int int5;
        [FieldOffset(24)] public int int6;
        [FieldOffset(28)] public int int7;
    }
    
    [StructLayout(LayoutKind.Explicit, Size = 28)]
    public readonly struct Bytes28
    {
        [FieldOffset(0)] public readonly int int0;
        [FieldOffset(4)] public readonly int int1;
        [FieldOffset(8)] public readonly int int2;
        [FieldOffset(12)] public readonly int int3;
        [FieldOffset(16)] public readonly int int4;
        [FieldOffset(20)] public readonly int int5;
        [FieldOffset(24)] public readonly int int6;
    }
}