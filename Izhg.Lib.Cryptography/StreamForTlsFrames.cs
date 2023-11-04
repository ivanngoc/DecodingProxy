using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.Cryptography.Infos;
using IziHardGames.Libs.Cryptography.Readers;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.Libs.Cryptography.Recording
{
    internal class StreamForTlsFrames : Stream
    {
        private List<TlsFrame> frames;
        private readonly Queue<TlsFrame> queue;
        private readonly ESide side;
        private TlsFrame? currentRead;
        private TlsFrame? currentWrite;
        private int offsetRead;
        private int offsetWrite;
        private readonly TlsReader tlsReader = new TlsReader();
        public override bool CanRead { get => true; }
        public override bool CanWrite { get => true; }
        public override bool CanSeek { get; }
        public override long Length { get; }
        public override long Position { get; set; }
#if DEBUG
        private static readonly object lockerShared = new object();
#endif
        public StreamForTlsFrames(List<TlsFrame> frames, ESide side) : base()
        {
            this.frames = frames;
            queue = new Queue<TlsFrame>(frames);
            this.side = side;
        }

        public override void Flush()
        {

        }
        #region Reads
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(new Span<byte>(buffer, offset, count));
        }
        public override int Read(Span<byte> buffer)
        {
            lock (lockerShared)
            {
#if DEBUG
                TlsFrame frameDebug = default;
#endif
                int lengthToRead = default;
                bool isPartialRead = false;
                lock (queue)
                {
                    if (currentRead is null)
                    {
                        REPEAT:
                        if (queue.TryDequeue(out TlsFrame result))
                        {
                            if (result.IsRequestHello()) goto REPEAT;
#if DEBUG
                            frameDebug = result;
#endif
                            int length = result.dataWholeFrame.Length;
                            lengthToRead = length > buffer.Length ? buffer.Length : length;
                            isPartialRead = length > lengthToRead;
                            if (isPartialRead)
                            {
                                currentRead = result;
                                var slice = result.GetMemSLice(0, lengthToRead);
                                slice.Span.CopyTo(buffer);
                                offsetRead = lengthToRead;
                            }
                            else
                            {
                                result.dataWholeFrame.Span.CopyTo(buffer);
                            }
                        }
                    }
                    else
                    {
#if DEBUG
                        frameDebug = currentRead;
#endif
                        int lengthFrame = currentRead.dataWholeFrame.Length;
                        int lengthLeftToRead = lengthFrame - offsetRead;
                        lengthToRead = lengthLeftToRead > buffer.Length ? buffer.Length : lengthLeftToRead;
                        isPartialRead = lengthLeftToRead > lengthToRead;

                        if (isPartialRead)
                        {
                            ReadOnlyMemory<byte> mem = currentRead.GetMemSLice(this.offsetRead, lengthToRead);
                            offsetRead += lengthToRead;
                            mem.Span.CopyTo(buffer);
                        }
                        else
                        {
                            var mem = currentRead.GetMemSLice(this.offsetRead, lengthLeftToRead);
                            mem.Span.CopyTo(buffer);
                            offsetRead = default;
                            currentRead = default;
                        }
                    }
                }
#if DEBUG
                Console.WriteLine($"Readed:{lengthToRead}. IsPartial:{isPartialRead}. {frameDebug.ToStringInfoAsClient()}");
#endif
                return lengthToRead;
            }
        }
        public override int ReadByte()
        {
            throw new NotImplementedException();
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            int value = Read(buffer.Span);
            return ValueTask.FromResult(value);
        }
        #endregion

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        #region Writes
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }
        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Console.WriteLine($"Write Length:{buffer.Length}");

            if (tlsReader.TryParseOnAppendData(buffer, out var frame))
            {
                if (side == ESide.Client)
                {
                    Console.WriteLine("Writed\t" + frame.ToStringInfoAsClient());
                }
                else if (side == ESide.Server)
                {

                    Console.WriteLine("Writed\t" + frame.ToStringInfoAsServer());
                }
                else
                {
                    throw new System.NotImplementedException();
                }
            }
            return ValueTask.CompletedTask;
        }
        #endregion

        public override void Close()
        {
            base.Close();
            tlsReader.Dispose();
            currentRead?.Dispose();
            currentWrite?.Dispose();
            currentRead = default;
            currentWrite = default;
        }

    }
}