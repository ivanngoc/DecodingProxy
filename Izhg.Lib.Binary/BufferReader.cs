﻿using System;
using System.Buffers.Binary;
using System.Net;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining), ToBigEndian]
        public static unsafe int ToInt32Size3(in ReadOnlySpan<byte> span)
        {
            int result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = span[2];
            pointer[1] = span[1];
            pointer[2] = span[0];
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), ToBigEndian]
        public static unsafe int ToInt32(byte v1, byte v2, byte v3)
        {
            int result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = v3;
            pointer[1] = v2;
            pointer[2] = v1;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), ToBigEndian]
        public static unsafe short ToShort(byte v1, byte v2)
        {
            short result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = v2;
            pointer[1] = v1;
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining), NetworkByteOrder]
        public static unsafe ushort ToUshortNetworkOrder(byte v1, byte v2)
        {
            ushort result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = v1;
            pointer[1] = v2;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), ToBigEndian]
        public static unsafe ushort ToUshort(byte v1, byte v2)
        {
            ushort result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = v2;
            pointer[1] = v1;
            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining), ToBigEndian]
        public static unsafe ushort ToUshort(in ReadOnlySpan<byte> bytes)
        {
            ushort result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = bytes[1];
            pointer[1] = bytes[0];
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), ToBigEndian]
        public static unsafe ushort ToUshortConsume(ref ReadOnlySpan<byte> bytes)
        {
            ushort result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = bytes[1];
            pointer[1] = bytes[0];
            bytes = bytes.Slice(2);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining), ToBigEndian]
        public static unsafe ushort ToUshortConsume(ref ReadOnlyMemory<byte> bytes)
        {
            var span = bytes.Span;
            ushort result = default;
            byte* pointer = (byte*)&result;
            pointer[0] = span[1];
            pointer[1] = span[0];
            bytes = bytes.Slice(2);
            return result;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public static T ToStruct<T>(byte[] buffer, int offset, int length) where T : unmanaged
        {
            var span = new Span<byte>(buffer, offset, length);
            return MemoryMarshal.Cast<byte, T>(span)[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToStruct<T>(in ReadOnlySpan<byte> readOnlySpan) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(readOnlySpan)[0];
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ToStruct<T>(in ReadOnlyMemory<byte> mem) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(mem.Span)[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T ToStructUnsafe<T>(void* readOnlySpan) where T : unmanaged
        {
            Span<T> span = new Span<T>(readOnlySpan, 1);
            return span[0];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T ToStructWithPointers<T>(in ReadOnlyMemory<byte> mem) where T : unmanaged
        {
            return ToStructWithPointers<T>(mem.Span);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T ToStructWithPointers<T>(in ReadOnlySpan<byte> span) where T : unmanaged
        {
            T result = default(T);
            byte* pointer = (byte*)&result;
            int length = sizeof(T);
            var size = Marshal.SizeOf<T>();
            for (int i = 0; i < length; i++)
            {
                pointer[i] = span[i];
            }
            return result;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReverseEndians(ushort value)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReverseEndians(uint value)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe static T ToStructConsume<T>(ref ReadOnlyMemory<byte> mem) where T : unmanaged
        {
            T st = ToStruct<T>(in mem);
            mem = mem.Slice(sizeof(T));
            return st;
        }

        public static ReadOnlyMemory<byte> Consume(int length, ref ReadOnlyMemory<byte> slice)
        {
            ReadOnlyMemory<byte> result = slice.Slice(0, length);
            slice = slice.Slice(length);
            return result;
        }

        public static byte ConsumeByte(ref ReadOnlyMemory<byte> slice)
        {
            var result = slice.Span[0];
            slice = slice.Slice(1);
            return result;
        }

        public static ReadOnlySpan<byte> ConsumeIPv4AsReadOnlySpan(ref ReadOnlyMemory<byte> slice)
        {
            var result = slice.Slice(0, 4).Span;
            slice = slice.Slice(4);
            return result;
        }
        public static ReadOnlySpan<byte> ConsumeIPv4AsLong(ref ReadOnlyMemory<byte> slice)
        {
            var result = slice.Slice(0, 4).Span;
            slice = slice.Slice(4);
            return result;
        }
        public static IPAddress ConsumeIPv4AsIPAddress(ref ReadOnlyMemory<byte> slice)
        {
            var span = slice.Slice(0, 4).Span;
            slice = slice.Slice(4);
            return new IPAddress(span);
        }
        public static IPAddress ConsumeIPv6AsIPAddress(ref ReadOnlyMemory<byte> slice)
        {
            var span = slice.Slice(0, 16).Span;
            slice = slice.Slice(16);
            return new IPAddress(span);
        }
    }
}