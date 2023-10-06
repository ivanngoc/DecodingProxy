using System;
using System.IO;

namespace IziHardGames.Libs.IO
{
    public class FileHelper
    {
        public unsafe static void AppendAllBytes16(string dir,string filename, void* header16, in ReadOnlyMemory<byte> data)
        {
            Span<byte> buffer = new Span<byte>(header16, 16);
            FileStream fileStream = default;
            string path = Path.Combine(dir, filename);
            if (!File.Exists(path))
            {
                fileStream = File.Create(path);
            }
            else
            {
                fileStream = File.OpenWrite(path);
            }
            fileStream.Seek(0, SeekOrigin.End);
            fileStream.Write(buffer);
            fileStream.Write(data.Span);
            fileStream.Close();
        }
        public static void AppendAllBytes(string dir, string filename, in ReadOnlyMemory<byte> data)
        {
            throw new System.NotImplementedException();
        }
    }
}
