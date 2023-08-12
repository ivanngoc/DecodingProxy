using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace IziHardGames.Libs.Binary.Readers
{
    public static class BufferReader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining), LittleEndian]
        public static int ConcatToInt32(byte v1, byte v2, byte v3)
        {
            return ((v1 << 16) | (v2 << 8) | v3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), LittleEndian]
        public static short ConcatToShort(byte v1, byte v2)
        {
            return (short)((v1 << 8) | v2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), BigEndian]
        public static unsafe int ToInt32(byte v1, byte v2, byte v3)
        {
            int result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = v3;
            pointer[1] = v2;
            pointer[2] = v1;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), BigEndian]
        public static unsafe short ToShort(byte v1, byte v2)
        {
            short result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = v2;
            pointer[1] = v1;
            return result;
        } 
        [MethodImpl(MethodImplOptions.AggressiveInlining), BigEndian]
        public static unsafe ushort ToUshort(byte v1, byte v2)
        {
            ushort result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = v2;
            pointer[1] = v1;
            return result;
        } 
        [MethodImpl(MethodImplOptions.AggressiveInlining), BigEndian]
        public static unsafe ushort ToUshort(ReadOnlySpan<byte> bytes)
        {
            ushort result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = bytes[1];
            pointer[1] = bytes[0];
            return result;
        }

       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static T ToStruct<T>(byte[] buffer, int offset, int length) where T : unmanaged
        {
            var span = new Span<byte>(buffer, offset, length);
            return MemoryMarshal.Cast<byte, T>(span)[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToStruct<T>(ReadOnlySpan<byte> readOnlySpan) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(readOnlySpan)[0];
        }
    }
}