using System;
using System.IO;
using System.IO.Compression;

namespace IziHardGames.Libs.Encodings
{
    public class Gzip
    {
        public static void Read(Stream input)
        {
            Span<byte> span = stackalloc byte[8192];

            using (GZipStream stream = new GZipStream(input, CompressionMode.Compress))
            {
                int str = stream.Read(span);
            }
        }
    }
}