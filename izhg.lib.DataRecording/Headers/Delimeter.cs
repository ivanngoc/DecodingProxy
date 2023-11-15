using System;
using System.Runtime.InteropServices;

namespace IziHardGames.DataRecording.Abstractions.Lib.Headers
{
    /// <summary>
    /// Делиметер
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public struct Delimeter
    {
        [FieldOffset(0)] public int index;
        [FieldOffset(4)] public int length;
        [FieldOffset(8)] public long dateTime;
        public Delimeter(int index, int length)
        {
            this.index = index;
            this.length = length;
            dateTime = DateTime.Now.Ticks;
        }

        public string ToStringInfo()
        {
            return $"index:{index}; length:{length}; datetime:{new DateTime(dateTime)}";
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 32)]
    public readonly struct DelimeterTyped
    {
        [FieldOffset(0)] public readonly Delimeter delimeter;
        [FieldOffset(16)] public readonly int type;
        [FieldOffset(20)] public readonly int id;
        [FieldOffset(24)] public readonly uint lengthDescription;

        [FieldOffset(28)] public readonly int reserved2;
    }
}
