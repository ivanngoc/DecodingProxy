// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames;
using ProxyLibs.Extensions;
using System.Buffers;
using System.Drawing;
using System.Net;

namespace HttpDecodingProxy.http
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9112.html#message.body
    /// </summary>
    [Serializable]
    public class HttpBody : IDisposable
    {
        private byte[] buffer = Array.Empty<byte>();
        private int bufferLength;
        private readonly List<byte[]> chunks = new List<byte[]>();
        [NonSerialized] public Memory<byte> datas;

        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#message.body.length
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public void ReadBody(Stream stream, HttpOject obj)
        {
            Http11Fields fields = obj.fields;

            if (fields.IsRequest)
            {
                if (fields.Method == WebRequestMethods.Http.Connect)
                {
                    return;
                }
            }
            if (fields.IsResponse)
            {
                if (obj.httpMessage.request.fields.IsMethod(WebRequestMethods.Http.Head)) return;
                if (fields.IsStatusCodeRange(Http.StatusCodes.INFORMATIONAL_100) || (fields.IsStatusCode(Http.StatusCodes.NO_CONTENT_204) || fields.IsStatusCode(Http.StatusCodes.NOT_MODIFIED_304))) return;

                if (obj.httpMessage.request.fields.Method == WebRequestMethods.Http.Connect && fields.IsStatusCodeRange(Http.StatusCodes.SUCCESSFUL_200)) return;
            }


            if (fields.TryFindTransferEncoding(out string[] encodings))
            {
                if (fields.IsChunkedLast())
                {
                    ReadChunk(stream, obj);
                }
                else
                {
                    // read until server close connection
                    throw new System.NotImplementedException();
                }
            }
            else
            {
                if (fields.TryFindContentLengthLine(out string contentLength))
                {
                    int size = int.Parse(contentLength.Substring(16));

                    if (size > 0)
                    {
                        fields.ContentLength = size;
                        fields.ContentLengthField = contentLength;

                        var buf = ArrayPool<byte>.Shared.Rent(size);
                        this.buffer = buf;

                        stream.ReadForSure(buf, 0, size);
                        bufferLength = size;
                        datas = new Memory<byte>(this.buffer, 0, bufferLength);
                    }
                }
                return;
                //else
                //{
                //    Logger.LogException(new NotImplementedException($"ReadBody not implemented:{Environment.NewLine}{obj.sb.ToString()}"));

                //    if (fields.Method == WebRequestMethods.Http.Connect)
                //    {
                //        while (true)
                //        {
                //            Console.Write((char)stream.ReadByte());
                //        }
                //    }
                //    TextReader tr = new StreamReader(stream);

                //    while (true)
                //    {
                //        Console.WriteLine(tr.ReadLine());
                //    }
                //}
            }
        }

        private static void ReadBodyAsDocument(Stream stream, Http11Fields fields)
        {
            throw new System.NotImplementedException();
        }

        private void ReadChunk(Stream stream, HttpOject obj)
        {
            Logger.LogLine($"Begin Read Chunk");

            var fields = obj.fields;
            buffer = ArrayPool<byte>.Shared.Rent((1 << 20) * 8);

            while (true)
            {
                string hex = stream.ReadLine(128, buffer, bufferLength, out int length);
                bufferLength += length;
                int size = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                // if last chunk
                if (size == 0) break;
                Logger.LogLine($"Size: {size}");

                EnsureSizeToAppend(size + 2);
                stream.ReadForSure(buffer, bufferLength, size);
                bufferLength += size;

                // read CRLF
                stream.ReadCRLF(buffer, bufferLength);
                bufferLength += 2;
            }

            // read  trailer-section
            // read  CRLF
            EnsureSizeToAppend(2);
            stream.ReadCRLF(buffer, bufferLength);
            bufferLength += 2;

            //TextReader tr = new StreamReader(stream);
            //while (true)
            //{
            //    Logger.LogLine(tr.ReadLine());
            //}
            datas = new Memory<byte>(buffer, 0, bufferLength);
            Logger.LogLine($"Read Chunk Completed");
        }

        private void EnsureSizeToAppend(int size)
        {
            int sizeTarget = bufferLength + size;
            if (buffer.Length >= (sizeTarget)) return;
            var newBuffer = ArrayPool<byte>.Shared.Rent(sizeTarget);
            Array.Copy(buffer, 0, newBuffer, 0, bufferLength);
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = newBuffer;
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(buffer);
            buffer = Array.Empty<byte>();
            bufferLength = default;
            chunks.Clear();
            datas = default;
        }

        internal void WriteTo(Stream stream)
        {
            if (datas.Length > 0)
            {
                stream.Write(datas.Span);
            }
        }
    }
}