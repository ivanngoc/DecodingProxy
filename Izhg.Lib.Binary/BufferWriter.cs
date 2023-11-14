using System;

namespace IziHardGames.Libs.Binary.Writers
{
    public static class BufferWriter
    {
        public static unsafe int WirteToBuffer(byte[] item, byte[] dest, int offset)
        {
            var slice = new Span<byte>(dest, offset, item.Length);
            item.AsSpan().CopyTo(slice);
            return item.Length;
        }
        public static unsafe int WirteToBuffer<T>(T item, byte[] bytes, int offset) where T : unmanaged
        {
            int length = sizeof(T);
            byte* pointer = (byte*)&item;
            for (int i = 0; i < length; i++)
            {
                bytes[offset + i] = pointer[i];
            }
            return length;
        }
        public static unsafe int WirteToBufferUshort(ushort value, byte[] bytes, int offset)
        {
            return WirteToBufferReverseEndians(value, bytes, offset);
        }
        public static unsafe int WirteToBufferInt(int value, byte[] bytes, int offset)
        {
            return WirteToBufferReverseEndians(value, bytes, offset);
        }
        public static unsafe int WirteToBufferReverseEndians<T>(T item, byte[] bytes, int offset) where T : unmanaged
        {
            int length = sizeof(T);
            int lastIndex = length - 1;
            byte* pointer = (byte*)&item;

            for (int i = 0; i < length; i++)
            {
                bytes[offset + i] = pointer[lastIndex - i];
            }
            return length;
        }
        public static void ToReadOnlySpan<T>(this T item, out ReadOnlySpan<byte> readOnlyMemory) where T : unmanaged
        {
            throw new NotImplementedException();
        }
        public static void ToReadOnlyMemory<T>(this T item, out ReadOnlyMemory<byte> readOnlyMemory) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public unsafe static byte[] ToArray<T>(T item) where T : unmanaged
        {
            int length = sizeof(T);
            byte[] bytes = new byte[length];
            byte* pointer = (byte*)&item;
            for (int i = 0; i < length; i++)
            {
                bytes[i] = pointer[i];
            }
            return bytes;
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