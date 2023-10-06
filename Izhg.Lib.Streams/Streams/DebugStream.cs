using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.DevTools
{
    public class DebugStream : Stream
    {
        private Stream innerStream;
        private ELogType logType;
        public readonly Guid guid = new Guid();

        private List<LogEntry> logsRead = new List<LogEntry>();
        private List<LogEntry> logsWrite = new List<LogEntry>();
        private MemoryStream memRead = new MemoryStream();
        private MemoryStream memWrite = new MemoryStream();

        public byte[] MemRead => memRead.ToArray();
        public byte[] MemWrite => memWrite.ToArray();

        public override bool CanRead { get => innerStream.CanRead; }
        public override bool CanSeek { get => innerStream.CanSeek; }
        public override bool CanWrite { get => innerStream.CanWrite; }
        public override long Length { get => Length; }
        public override long Position { get => innerStream.Position; set => innerStream.Position = value; }

        public DebugStream(Stream stream, ELogType eLogType)
        {
            innerStream = stream;
            logType = eLogType;
        }
        public override void Flush()
        {
            innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readed = innerStream.Read(buffer, offset, count);
            memRead.Write(buffer, offset, readed);
            LogRead(new ReadOnlySpan<byte>(buffer, offset, readed));
            return readed;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public async override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var readed = await innerStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            await memRead.WriteAsync(buffer.Slice(0, readed)).ConfigureAwait(false);
            LogRead(buffer.Slice(0, readed).Span);
            return readed;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            LogWrite(new ReadOnlySpan<byte>(buffer, offset, count));
            memWrite.Write(buffer, offset, count);
            innerStream.Write(buffer, offset, count);
        }
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
        public async override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            LogWrite(buffer.Span);
            await memWrite.WriteAsync(buffer).ConfigureAwait(false);
            await innerStream.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            throw new NotImplementedException();
        }
        public override void WriteByte(byte value)
        {
            throw new NotImplementedException();
        }
        protected void LogWrite(ReadOnlySpan<byte> span)
        {
            switch (logType)
            {
                case ELogType.None:
                    break;
                case ELogType.Binary:
                    break;
                case ELogType.HexFormat:
                    {
                        if (span.Length > 0)
                        {
                            Log(ParseByte.ToHexStringFormated(span), logsWrite, LogEntry.TYPE_WRITE);
                        }
                        else
                        {
                            Console.WriteLine($"Zero Length Write");
                            //throw ZeroWriteException.shared;
                        }
                        //Console.Write($"\r\n");
                        break;
                    }
                default: break;
            }
        }
        protected void LogRead(ReadOnlySpan<byte> span)
        {
            switch (logType)
            {
                case ELogType.None:
                    break;
                case ELogType.Binary:
                    break;
                case ELogType.HexFormat:
                    {
                        if (span.Length > 0)
                        {
                            Log(ParseByte.ToHexStringFormated(span), logsRead, LogEntry.TYPE_READ);
                        }
                        else
                        {
                            Console.WriteLine($"Zero length read");
                        }
                        //Console.Write("\r\n");
                        break;
                    }
                default: break;
            }
        }

        private void Log(string msg, List<LogEntry> container, int type)
        {
            lock (this)
            {
                LogEntry entry = new LogEntry()
                {
                    dateTime = DateTime.Now,
                    index = container.Count,
                    message = msg,
                    type = type,
                };
                container.Add(entry);
            }
        }
        public override void Close()
        {
            innerStream.Close();
            memRead.Dispose();
            memWrite.Dispose();
        }

        public string ToLog()
        {
            return $"{guid}{Environment.NewLine}" + logsRead.Concat(logsWrite).OrderBy(x => x.dateTime).Select(x => x.ToString()).Aggregate((x, y) => x + Environment.NewLine + y);
        }
    }

    public class LogEntry
    {
        public const int TYPE_READ = 1;
        public const int TYPE_WRITE = 2;
        public DateTime dateTime;
        public string message;
        public int index;
        public int type;

        public override string ToString()
        {
            return $"{DateTime.Now.ToString("HH:mm:ss.fffffff")}    {message}";
        }
    }

    public enum ELogType
    {
        None,
        Binary,
        HexFormat,
    }
}