using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Proxy.TcpDecoder;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    public class TlsHandshakeReader : IDisposable
    {
        private byte[] buffer = new byte[8092];
        private int offset;
        private int lengthLeftToCopy;

        private int handshakeLength;
        private int LeftFree => buffer.Length - offset;
        private TaskCompletionSource? tcs;
        private Task? awaitingTask;
        private bool isDisposed = true;
        public readonly Func<ReadOnlyMemory<byte>, bool> actionAddDataWithCheck;

        public TlsHandshakeReader()
        {
            actionAddDataWithCheck = (x) => AddDataWithCheck(in x);
        }

        public void Initilize()
        {
            if (!isDisposed) throw new ObjectDisposedException($"Object must be disposed before use");
            isDisposed = false;
            tcs = new TaskCompletionSource();
            awaitingTask = tcs.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mem"></param>
        /// <returns>
        /// Amount Of Copied Data
        /// </returns>
        public int AddData(in ReadOnlyMemory<byte> mem)
        {
            var left = LeftFree;
            int toCopy = left > mem.Length ? mem.Length : left;
            mem.CopyTo(new Memory<byte>(buffer, offset, toCopy));
            offset += toCopy;
            return toCopy;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mem"></param>
        /// <returns>
        /// Amount Of Copied Data
        /// </returns>
        public int AddData(in Memory<byte> mem)
        {
            var left = LeftFree;
            int toCopy = left > mem.Length ? mem.Length : left;
            mem.CopyTo(new Memory<byte>(buffer, offset, toCopy));
            offset += toCopy;
            return toCopy;
        }
        public bool AddDataWithCheck(in ReadOnlyMemory<byte> mem)
        {
            //Console.WriteLine($"[{GetType().Name}]: [{ParseByte.ToHexStringFormated(mem)}]");
            if (offset < 5)
            {
                if (mem.Length > 4)
                {
                    var span = mem.Span;
                    handshakeLength = BufferReader.ToUshort(span[3], span[4]);
                    lengthLeftToCopy = 5 + handshakeLength - offset;
                    int toCopy = mem.Length > lengthLeftToCopy ? lengthLeftToCopy : mem.Length;
                    lengthLeftToCopy -= toCopy;
                    mem.Slice(0, toCopy).CopyTo(new Memory<byte>(buffer, offset, toCopy));
                    offset += toCopy;
                }
                else
                {
                    mem.CopyTo(new Memory<byte>(buffer, offset, mem.Length));
                    offset += mem.Length;
                    return false;
                }
            }
            else
            {
                int toCopy = mem.Length > lengthLeftToCopy ? lengthLeftToCopy : mem.Length;
                lengthLeftToCopy -= toCopy;
                mem.Slice(0, toCopy).CopyTo(new Memory<byte>(buffer, offset, toCopy));
                offset += toCopy;
            }

            if (lengthLeftToCopy == 0 && offset > 0)
            {
                tcs.SetResult();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Провервка на целостность
        /// </summary>
        /// <returns></returns>
        public bool CheckIntegrity()
        {
            if (buffer[0] != ConstantsTls.HANDSHAKE_RECORD) throw new FormatException();
            handshakeLength = BufferReader.ToUshort(buffer[3], buffer[4]);
            return !(this.offset < (this.offset - 5));
        }

        public static bool CheckIntegrity(in ReadOnlyMemory<byte> mem)
        {
            if (mem.Length < 5) return false;
            var span = mem.Span;
            if (span[0] != ConstantsTls.HANDSHAKE_RECORD) throw new FormatException("First Byte Must be = 0x16");
            var length = BufferReader.ToUshort(span[3], span[4]);
            return (length + 5) <= mem.Length;
        }

        public static bool CheckIntegrity(in ReadOnlySequence<byte> data, out ushort length)
        {
            length = default;
            if (data.Length < 5) return false;

            if (data.IsSingleSegment)
            {
                var span = data.FirstSpan;
                if (span[0] != ConstantsTls.HANDSHAKE_RECORD) throw new FormatException($"First Byte Must be 0x16");
                length = BufferReader.ToUshort(span[3], span[4]);
                return (5 + length) <= data.Length;
            }
            else
            {
                if (data.GetItemAt(0) != ConstantsTls.HANDSHAKE_RECORD) throw new FormatException($"First Byte Must be 0x16");
                length = BufferReader.ToUshort(data.GetItemAt(3), data.GetItemAt(4));
                return (5 + length) <= data.Length;
            }
        }

        public ReadOnlyMemory<byte> GetBuffer()
        {
            return new ReadOnlyMemory<byte>(buffer, 0, offset);
        }

        public async Task AwaitHandShake()
        {
            if (tcs == null) throw new NullReferenceException($"{nameof(TlsHandshakeReader)} wasn't initilized yet");
            await awaitingTask.ConfigureAwait(false);
            tcs = default;
            awaitingTask = default;
        }

        public void Dispose()
        {
            if (isDisposed) throw new ObjectDisposedException("Object is already disposed");
            isDisposed = true;
            if (tcs != default)
            {
                tcs = default;
                if (awaitingTask.Status != TaskStatus.RanToCompletion)
                {
                    throw new TaskCanceledException("Task must complete before dispose");
                }
            }
            offset = default;
            lengthLeftToCopy = default;
            handshakeLength = default;
        }

        public ReadOnlyMemory<byte> GetFrameAsMemory()
        {
            return new ReadOnlyMemory<byte>(buffer, 0, offset);
        }
    }
}
