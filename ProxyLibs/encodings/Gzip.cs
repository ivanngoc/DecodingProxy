// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System;
using System.IO.Compression;
using System.Net.Security;

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