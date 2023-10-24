using System;
using System.Runtime.InteropServices;
using System.Text;
using Izhg.Libs.Mapping;
using IziHardGames.Libs.Binary.Writers;
using IziHardGames.Libs.Buffers.Vectors;

namespace DevConsole.Shared.Consoles
{
    [StructLayout(LayoutKind.Explicit, Size = 6)]
    public struct Header
    {
        /// <summary>
        /// <see cref="ConstantsForConsoles.MAGIC_NUMBER_HEADER"/>
        /// </summary>
        [FieldOffset(0)] public int magicNumber;
        [FieldOffset(4)] public ushort length;
        [FieldOffset(6)] public int type;

        public override string ToString()
        {
            return $"{magicNumber}\t{length}\t{type}";
        }
    }


    [StructLayout(LayoutKind.Explicit, Size = ConstantsForConsoles.SIZE_LOG_HEADER)]
    public readonly struct LogHeader
    {
        /// <summary>
        /// <see cref="ConstantsForConsoles.MAGIC_NUMBER_LOG_HEADER"/>
        /// </summary>
        [FieldOffset(0)] public readonly int magicNumber;
        [FieldOffset(4)] public readonly int id;
        [FieldOffset(8)] public readonly uint order;
        [FieldOffset(12)] public readonly EContentType type;
        [FieldOffset(16)] public readonly uint length;
        [FieldOffset(20)] public readonly int groupe;
        [FieldOffset(24)] public readonly long dateTimeTicks;

        public LogHeader(int id, uint order, EContentType type, uint length, int groupe, long dateTimeTicks)
        {
            this.magicNumber = ConstantsForConsoles.MAGIC_NUMBER_LOG_HEADER;
            this.id = id;
            this.order = order;
            this.type = type;
            this.length = length;
            this.groupe = groupe;
            this.dateTimeTicks = dateTimeTicks;
        }

        public DateTime DateTime => new DateTime(dateTimeTicks);

        /// <summary>
        /// Base64 encoding?
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public static LogHeader FromString(string s)
        {
            throw new System.NotImplementedException();
        }
        public static LogHeader FromReadOnlyMemory(in ReadOnlyMemory<byte> bytes)
        {
            throw new System.NotImplementedException();
        }
        public static ArraySegmentDisposable ForString(string s)
        {
            throw new System.NotImplementedException();
        }
        internal unsafe static ArraySegmentDisposable FromSelf(LogHeader header)
        {
            var seg = new ArraySegmentDisposable(ConstantsForConsoles.SIZE_LOG_HEADER);
            var value = seg.array;
            ArrayWriter writer = new ArrayWriter(value, 0);
            writer.WriteStruct<LogHeader>(&header);
            return seg;
        }
        internal unsafe static ArraySegmentDisposable FromSelf(void* header)
        {
            var seg = new ArraySegmentDisposable(ConstantsForConsoles.SIZE_LOG_HEADER);
            var value = seg.array;
            ArrayWriter writer = new ArrayWriter(value, 0);
            writer.WriteStruct<LogHeader>((void*)header);
            return seg;
        }
        internal static ArraySegmentDisposable ForBytes(int length, Encoding encoding)
        {
            var header = new ArraySegmentDisposable(ConstantsForConsoles.SIZE_LOG_HEADER);
            var value = header.array;
            ArrayWriter writer = new ArrayWriter(value, 0);
            writer.Write(length);

            if (encoding == Encoding.UTF8)
            {
            }
            throw new System.NotImplementedException();
        }
    }


}