using System;
using System.Buffers;

namespace IziHardGames.Libs.IO
{
    public static class Utf8
    {
        public const byte LEADING_1_BYTE = 0;                  //0
        public const byte LEADING_2_BYTE = 0b1100_0000;        //110
        public const byte LEADING_3_BYTE = 0b1110_0000;        //1110
        public const byte LEADING_4_BYTE = 0b1111_0000;        //11110

        // this masks using for extract value from
        public const int MASK_UTF8_BYTE1 = 0b_1000_0000_0000_0000; // 0xxx xxxx
        public const int MASK_UTF8_BYTE2 = 0b_1110_0000_0000_0000; // 110x xxxx
        public const int MASK_UTF8_BYTE3 = 0b_1111_0000_0000_0000; // 1110 xxxx
        public const int MASK_UTF8_BYTE4 = 0b_1111_1000_0000_0000; // 1111 0xxx 
        public static byte GetSize(byte leadingByte)
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
}
