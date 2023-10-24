using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Streams
{
    public class MyTextWriter : TextWriter
    {
        public override Encoding Encoding { get => encoding; }
        private Encoding encoding = Encoding.UTF8;
        private Stream innerStream;

        public MyTextWriter(Stream stream)
        {
            this.innerStream = stream;
        }

        public override void Write(bool value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(char value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(char[] buffer, int index, int count)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(char[]? buffer)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(decimal value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(double value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(float value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(int value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(long value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(object? value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(string format, object? arg0)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(string format, object? arg0, object? arg1)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(string format, object? arg0, object? arg1, object? arg2)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(string format, params object?[] arg)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(string? value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(StringBuilder? value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(ReadOnlySpan<char> buffer)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(uint value)
        {
            throw new System.NotImplementedException();
        }
        public override void Write(ulong value)
        {
            throw new System.NotImplementedException();
        }



        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteAsync(char value)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteAsync(string? value)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }



        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteLineAsync(char value)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteLineAsync(string? value)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
        public override Task WriteLineAsync()
        {
            throw new System.NotImplementedException();
        }


        public override void WriteLine()
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(bool value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(char value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(char[] buffer, int index, int count)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(char[]? buffer)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(decimal value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(double value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(float value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(int value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(long value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(object? value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(string format, object? arg0)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(string format, object? arg0, object? arg1)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(string format, object? arg0, object? arg1, object? arg2)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(string format, params object?[] arg)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                innerStream.Write(StreamForLines.rn);
            }
            else
            {
                innerStream.Write(encoding.GetBytes(value));
                innerStream.Write(StreamForLines.rn);
            }
        }
        public override void WriteLine(StringBuilder? value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(uint value)
        {
            throw new System.NotImplementedException();
        }
        public override void WriteLine(ulong value)
        {
            throw new System.NotImplementedException();
        }


        public override void Flush()
        {
            throw new System.NotImplementedException();
        }
        public override Task FlushAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}