using System.Collections.Generic;

namespace System
{
    public static class ParseByte
    {
        public static readonly Dictionary<char, byte> charsByByte = new Dictionary<char, byte>()
        {
            ['0'] = 0,
            ['1'] = 1,
            ['2'] = 2,
            ['3'] = 3,
            ['4'] = 4,
            ['5'] = 5,
            ['6'] = 6,
            ['7'] = 7,
            ['8'] = 8,
            ['9'] = 9,
            ['A'] = 10,
            ['B'] = 11,
            ['C'] = 12,
            ['D'] = 13,
            ['E'] = 14,
            ['F'] = 15,
        };
        public static readonly Dictionary<int, char> charsPerBytes = new Dictionary<int, char>()
        {
            [0] = '0',
            [1] = '1',
            [2] = '2',
            [3] = '3',
            [4] = '4',
            [5] = '5',
            [6] = '6',
            [7] = '7',
            [8] = '8',
            [9] = '9',
            [10] = 'A',
            [11] = 'B',
            [12] = 'C',
            [13] = 'D',
            [14] = 'E',
            [15] = 'F',
        };

        public static byte GetByteByChar(char c)
        {
            return charsByByte[c];
        }

        public static string ByteToHexFormated(byte b)
        {
            var pair = ByteToHex(b);
            return $"0x{pair.Item1}{pair.Item2} ";
        }
        public static (char, char) ByteToHex(byte b)
        {
            var first = b >> 4;
            var second = b & 0b_0000_1111;
            return (charsPerBytes[first], charsPerBytes[second]);
        }

        public  static string ToHexStringFormated(ReadOnlySpan<byte> type)
        {
            int size = type.Length;
            // 0x[char0][char1][SPACE]
            Span<char> chars = stackalloc char[(size << 2) + size];
            int k = default;
            for (int i = 0; i < size; i++, k += 5)
            {
                var pair = ByteToHex(type[i]);
                chars[k] = '0';
                chars[k + 1] = 'x';
                chars[k + 2] = pair.Item1;
                chars[k + 3] = pair.Item2;
                chars[k + 4] = ' ';
            }
            return new string(chars);
        }
        public unsafe static string ToHexStringFormated<T>(T type) where T : unmanaged
        {
            byte* pointer = (byte*)&type;
            int size = sizeof(T);
            // 0x[char0][char1][SPACE]
            Span<char> chars = stackalloc char[(size << 2) + size];
            int k = default;
            for (int i = 0; i < size; i++, k += 5)
            {
                var pair = ByteToHex(pointer[i]);
                chars[k] = '0';
                chars[k + 1] = 'x';
                chars[k + 2] = pair.Item1;
                chars[k + 3] = pair.Item2;
                chars[k + 4] = ' ';
            }
            return new string(chars);
        }
    }
}