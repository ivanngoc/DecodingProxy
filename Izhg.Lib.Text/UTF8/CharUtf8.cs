using System;
using System.Buffers;

namespace IziHardGames.Libs.IO
{

    public static class ConstantsUtf8
    {
        public const byte SPACE = 0b_0010_0000;     // 32
        public const byte COMMA = 0b_0010_1100;     // 44
        public const byte EQUALS = 0b_0011_1101;    // 61
    }

    public static class Utf8
    {
        // this masks using for extract value from
        public const int MASK_UTF8_BYTE1 = 0b_1000_0000_0000_0000; // 0xxx xxxx
        public const int MASK_UTF8_BYTE2 = 0b_1110_0000_0000_0000; // 110x xxxx
        public const int MASK_UTF8_BYTE3 = 0b_1111_0000_0000_0000; // 1110 xxxx
        public const int MASK_UTF8_BYTE4 = 0b_1111_1000_0000_0000; // 1111 0xxx 
        public static int GetSize(byte leadingByte)
        {
            if (leadingByte < 0b_1000_0000) return 1;
            if (leadingByte < 0b_1110_0000) return 2;
            if (leadingByte < 0b_1111_0000) return 3;
            if (leadingByte < 0b_1111_1000) return 4;
            throw new System.ArgumentException("Argument is not leading byte");
        }

        ///<see cref="System.Text.Unicode.Utf8.ToUtf16"/>
        public static int FillCharSpan(ReadOnlySequence<byte> seq, ref Span<char> span)
        {
            int bytesConsumed;
            int count = default;
            int size = default;

            for (int i = 0; i < count; i += size, i++)
            {
            }
            throw new System.NotImplementedException();
            return bytesConsumed;
        }
    }

    public struct CharUtf8
    {

    }
}
