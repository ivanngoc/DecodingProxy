using System;
using System.IO;
using System.Threading;

namespace IziHardGames.Libs.DevTools
{
    public class SocketDebugger
    {
        private string title;
        private int sorting;
        private int id;
        private string filename;
        private static int counter = 0;

        internal void Init(int id, string title)
        {
            return;
            var val = Interlocked.Increment(ref counter);
            sorting = val;

            this.id = id;
            this.title = title;
            filename = $"records/{sorting} {DateTime.Now.ToString("HH.mm.ss.ffffff")}_{id}_{title}_record.txt";

            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }
            if (!File.Exists(filename))
            {
                File.Create(filename).Dispose();
            }
        }

        internal void Push(int bytesRead, Memory<byte> memory)
        {
            return;
            var fs = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            fs.Seek(0, SeekOrigin.End);
            fs.Write(memory.Span.Slice(0, bytesRead));
            fs.Dispose();
        }
    }
}