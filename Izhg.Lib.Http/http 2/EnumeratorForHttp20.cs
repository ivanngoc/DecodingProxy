using System;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.HttpCommon.Http20;

namespace IziHardGames.Libs.ForHttp20.Maps
{
    public struct EnumerabableFromBufferForHttp20
    {
        private ReadOnlyMemory<byte> memory;

        public EnumerabableFromBufferForHttp20(in ReadOnlyMemory<byte> memory)
        {
            this.memory = memory;
        }
        public EnumeratorFromBufferForHttp20 GetEnumerator()
        {
            return new EnumeratorFromBufferForHttp20(memory);
        }
    }

    public struct EnumeratorFromBufferForHttp20
    {
        private ReadOnlyMemory<byte> mem;
        public MapMessageHttp20 Current { get; set; }
        public EnumeratorFromBufferForHttp20(in ReadOnlyMemory<byte> memory) : this()
        {
            this.mem = memory;
        }

        public bool MoveNext()
        {
			var frame = BufferReader.ToStruct<FrameHttp20>(mem.Span);
			int length = frame.length;
			Console.WriteLine($"Passed. Length:{length}. Type:{frame.GetTypeName()}. flags:{frame.flags}. stream ID:{frame.streamIdentifier}");
			mem = mem.Slice(ConstantsForHttp20.FRAME_SIZE);
			var payload = mem.Slice(0, length);
            Current = new MapMessageHttp20(frame, payload);
            return true;
		}
    }
}
