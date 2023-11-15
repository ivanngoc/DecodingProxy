using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using IziHardGames.DataRecording.Abstractions.Lib;
using IziHardGames.DataRecording.Abstractions.Lib.Headers;
using IziHardGames.Libs.IO;
using ProxyLibs.Extensions;
using Func = System.Func<System.ReadOnlyMemory<byte>, bool>;

namespace IziHardGames.Libs.Streams
{
    

    public class StreamForRecording : Stream
    {
        public override bool CanRead { get => innerStream!.CanRead; }
        public override bool CanSeek { get => innerStream!.CanSeek; }
        public override bool CanWrite { get => innerStream!.CanWrite; }
        public override long Length { get => innerStream!.Length; }
        public override long Position { get => innerStream!.Position; set => innerStream!.Position = value; }

        private string dir;
        private Stream? innerStream;
        public readonly Func actionRecordReader;
        public readonly Func actionRecordWriter;
        private string filename;
        private int countHeadersWriter;
        private int countHeadersReader;
        private static int counter;

        public StreamForRecording() : base()
        {
            actionRecordReader = RecordReader;
            actionRecordWriter = RecordWriter;
        }

        private unsafe bool RecordWriter(ReadOnlyMemory<byte> arg)
        {
            //return false;
            var header = new Delimeter(countHeadersWriter, arg.Length);
            lock (actionRecordReader)
            {
                FileHelper.AppendAllBytes16(dir, filename + ".writer", &header, in arg);
            }
            countHeadersWriter++;
            return false;
        }

        private unsafe bool RecordReader(ReadOnlyMemory<byte> arg)
        {
            //return false;
            var header = new Delimeter(countHeadersReader, arg.Length);
            lock (actionRecordWriter)
            {
                FileHelper.AppendAllBytes16(dir, filename + ".reader", &header, in arg);
            }
            countHeadersReader++;
            return false;
        }

        public void Initilize(string id, string dir, Stream stream)
        {
            var key = Interlocked.Increment(ref counter);
            this.dir = dir;
            innerStream = stream;
            filename = $"{key} {id}";
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            base.Close();
            this.innerStream = default;
        }
    }
}
