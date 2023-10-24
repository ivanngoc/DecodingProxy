using System;

namespace IziHardGames.Libs.Binary.Writers
{
    public static class BufferWriter
    {
        public static void ToReadOnlySpan<T>(this T item, out ReadOnlySpan<byte> readOnlyMemory) where T : unmanaged
        {
            throw new NotImplementedException();
        }
        public static void ToReadOnlyMemory<T>(this T item, out ReadOnlyMemory<byte> readOnlyMemory) where T : unmanaged
        {
            throw new NotImplementedException();
        }
    }

    public struct ArrayWriter
    {
        public byte[] array;
        public int offset;

        public ArrayWriter(byte[] array, int offset)
        {
            this.array = array;
            this.offset = offset;
        }

        public unsafe void Write(int value)
        {
            var pointer = (byte*)&value;
            array[offset] = pointer[3];
            offset++;
            array[offset] = pointer[2];
            offset++;
            array[offset] = pointer[1];
            offset++;
            array[offset] = pointer[0];
            offset++;
        }

        public unsafe void WriteStruct<T>(void* pointer) where T : unmanaged
        {
            int length = sizeof(T);
            int lengthEnd = length + offset;
            Span<byte> span = new Span<byte>(pointer, length);
            for (int i = offset; i < length; i++)
            {
                array[i] = span[i];
            }
        }
    }
}