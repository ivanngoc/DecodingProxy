// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System;
using System.IO;
using System.Net.Sockets;
using System.Xml.Linq;

namespace ProxyLibs.Extensions
{
    public static class ExtensionsForStream
    {

        /// <summary>
        /// Programmer sure that stream has got next byte
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int PeekUnsafe(this Stream s)
        {
            int value = s.ReadByte();
            s.Position--;
            return value;
        }

        public static void ReadForSure(this Stream s, byte[] buffer, int indexStart, int leftRead)
        {
            while (leftRead > 0)
            {
                int readed = s.Read(buffer, indexStart, leftRead);

                if (readed == leftRead) return;

                if (readed > 0)
                {
                    leftRead -= readed;
                    indexStart += readed;
                }
            }
            throw new IndexOutOfRangeException();
        }

        public static string ReadLine(this Stream s, int maxLength)
        {
            Span<char> buffer = stackalloc char[maxLength];
            int i = default;

            while (maxLength > 0)
            {
                maxLength--;
                buffer[i] = (char)s.ReadByte();

                if (buffer[i] == '\r')
                {
                    var c = (char)s.ReadByte();
                    if (c == '\n')
                    {
                        return new string(buffer.Slice(0, i));
                    }
                    else
                    {
                        i++;
                        buffer[i] = c;
                    }
                }
                i++;
            }
            return string.Empty;
        }

        public static string ReadLine(this Stream s, char[] buffer, out int bufferLength)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (char)s.ReadByte();

                if (buffer[i] == '\r')
                {
                    var c = (char)s.ReadByte();
                    i++;
                    buffer[i] = c;

                    if (c == '\n')
                    {
                        bufferLength = i;
                        return new string(buffer, 0, i - 1);
                    }
                }
            }
            throw new OverflowException($"Buffer size:{buffer.Length} was not enoguh");
        }

        public static void ReadCRLF(this Stream s, byte[] copyInto, int startIndex)
        {
            var cr = s.ReadByte();
            var lf = s.ReadByte();
            copyInto[startIndex] = (byte)cr;
            copyInto[startIndex + 1] = (byte)lf;
            if (cr != '\r' || lf != '\n') throw new ArgumentOutOfRangeException();
        }
        public static string ReadLine(this Stream s, int maxLength, byte[] buffer, int startIndex, out int bufferLength)
        {
            Span<char> span = stackalloc char[maxLength];
            int k = default;

            for (int i = startIndex; i < startIndex + maxLength; i++)
            {
                int b = s.ReadByte();
                buffer[i] = (byte)b;
                span[k] = (char)b;

                if (span[k] == '\r')
                {
                    b = s.ReadByte();
                    i++;
                    buffer[i] = (byte)b;

                    if (b == '\n')
                    {
                        var result = new string(span.Slice(0, k));
                        bufferLength = k + 2;
                        return result;
                    }
                    else
                    {
                        k++;
                        span[k] = (char)b;
                    }
                }
                k++;
            }
            throw new OverflowException($"Buffer size:{buffer.Length} was not enoguh");
        }
    }
}