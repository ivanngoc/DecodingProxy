using System;
using System.Buffers;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Infos;
using IziHardGames.Libs.Cryptography.Shared.Headers;

namespace IziHardGames.Libs.Cryptography.Readers
{
    /// <summary>
    /// Raw Reader
    /// </summary>
    public class TlsReader : IDisposable
    {
        protected internal const int BUFFER_SIZE = 1024;
        protected byte[] frame;
        /// <summary>
        /// Length of current payload
        /// </summary>
        protected int offset;
        private int lengthLeftFree;
        public TlsRecord RecordPeaked
        {
            get
            {
                if (frame.Length >= ConstantsForTls.SIZE_RECORD) return BufferReader.ToStruct<TlsRecord>(frame.AsMemory());
                return default;
            }
        }

        public TlsReader()
        {
            EnsureCapacity(BUFFER_SIZE);
        }

        public bool TryParseOnAppendData(in ReadOnlyMemory<byte> readOnlyMemory, out TlsFrame? tlsFrame)
        {
            tlsFrame = default;
            EnsureCapacity(readOnlyMemory.Length);
            readOnlyMemory.CopyTo(frame.AsMemory().Slice(offset));
            int lengthToAppend = readOnlyMemory.Length;
            offset += lengthToAppend;
            lengthLeftFree -= lengthToAppend;
            if (offset >= ConstantsForTls.SIZE_RECORD)
            {
                var mem = this.frame.AsMemory();
                TlsRecord header = BufferReader.ToStruct<TlsRecord>(mem);
                int frameSize = header.Length + ConstantsForTls.SIZE_RECORD;
                if (offset >= frameSize)
                {
                    tlsFrame = new TlsFrame(in header, mem.Slice(0, frameSize));
                    return true;
                }
            }
            return false;
        }

        private void EnsureCapacity(int lengthToAdd)
        {
            if (lengthToAdd > lengthLeftFree)
            {
                var newBuffer = ArrayPool<byte>.Shared.Rent(lengthToAdd + offset);
                if (frame is not null)
                {
                    Array.Copy(frame, 0, newBuffer, 0, offset);
                    ArrayPool<byte>.Shared.Return(frame);
                }
                this.frame = newBuffer;
                lengthLeftFree = newBuffer.Length - offset;
            }
        }

        public void Dispose()
        {

        }
    }
}
