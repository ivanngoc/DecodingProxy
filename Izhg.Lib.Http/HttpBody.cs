using IziHardGames;
using IziHardGames.Libs.NonEngine.Enumerators;
using ProxyLibs.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace HttpDecodingProxy.ForHttp
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9112.html#message.body
    /// </summary>
    [Serializable]
    public class HttpBody : IDisposable
    {
        public const int DEFAULT_SIZE_FIRST_LINE = 128;
        public const int INITIAL_BUFFER_SIZE = (1 << 20) * 8;

        private byte[] buffer = Array.Empty<byte>();
        private int bufferLength;
        private readonly List<byte[]> chunks = new List<byte[]>();
        [NonSerialized] public Memory<byte> datas;
        internal int leftToRead;
        internal int completed;
        public bool isCompleted;
        public bool isChunked;
        public bool isRedingChunk;
        public bool isRedingEnclosure;

        public string Utf8 => Encoding.UTF8.GetString(datas.Span);
        private static readonly object lockWrite = new object();

        private MemoryStream memoryStream;

        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#message.body.length
        /// <see cref="HttpPipedIntermediary.AwaitBody(HttpBinary, IziHardGames.Libs.Networking.Pipelines.TcpClientPiped)"/>
        /// <see cref="HttpObject.FillBodyChunked(in ReadOnlySequence{byte})"/>
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        public void ReadBody(Stream stream, HttpObject obj)
        {
            HttpFieldsV11 fields = obj.fields;

            if (fields.IsRequest)
            {
                if (fields.Method == WebRequestMethods.Http.Connect)
                {
                    return;
                }
            }
            if (fields.IsResponse)
            {
                if (obj.bind.request.fields.IsMethod(WebRequestMethods.Http.Head)) return;
                if (fields.IsStatusCodeRange(ConstantsForHttp.StatusCodes.INFORMATIONAL_100) || (fields.IsStatusCode(ConstantsForHttp.StatusCodes.NO_CONTENT_204) || fields.IsStatusCode(ConstantsForHttp.StatusCodes.NOT_MODIFIED_304))) return;

                if (obj.bind.request.fields.Method == WebRequestMethods.Http.Connect && fields.IsStatusCodeRange(ConstantsForHttp.StatusCodes.SUCCESSFUL_200)) return;
            }


            if (fields.TryFindTransferEncoding(out string[] encodings))
            {
                if (fields.IsChunkedLast())
                {
                    ReadChunks(stream, obj);
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
            }
        }

        private static void ReadBodyAsDocument(Stream stream, HttpFieldsV11 fields)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#chunked.encoding
        /// </summary>
        /// <example>
        /*
            HTTP/1.1 200 OK
            Content-Type: text/plain
            Transfer-Encoding: chunked
            Date: Wed, 12 Apr 2023 00:08:35 GMT

            25;some-extension-name=some-value
            This is the data in the first chunk

            1C;another-extension-name=another-value
            and this is the second one

            0

        */
        /// </example>
        /// <param name="stream"></param>
        /// <param name="obj"></param>
        private void ReadChunks(Stream stream, HttpObject obj)
        {
            MyLogger.LogLine($"Begin Read Chunk");

            var fields = obj.fields;
            buffer = ArrayPool<byte>.Shared.Rent(INITIAL_BUFFER_SIZE);

            while (true)
            {
                string hexSizeAndExtensions = stream.ReadLine(DEFAULT_SIZE_FIRST_LINE, buffer, bufferLength, out int length);
                if (hexSizeAndExtensions.Contains(';')) throw new System.NotImplementedException("There is no implementation for chunk extensions");
                string hexSize = hexSizeAndExtensions;
                bufferLength += length;
                int size = int.Parse(hexSize, System.Globalization.NumberStyles.HexNumber);
                // if last chunk
                if (size == 0) break;
                MyLogger.LogLine($"Size: {size}");

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
            MyLogger.LogLine($"Read Chunk Completed");
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
            if (buffer.Length > 0) ArrayPool<byte>.Shared.Return(buffer);
            buffer = Array.Empty<byte>();

            bufferLength = default;
            chunks.Clear();
            datas = default;
            leftToRead = default;
            isChunked = false;
            isCompleted = false;
            isRedingChunk = default;
            isRedingEnclosure = default;
            completed = 0;

            if (memoryStream != null)
            {
                memoryStream.Dispose();
                memoryStream = default;
            }
        }

        internal void WriteTo(Stream stream)
        {
            if (datas.Length > 0)
            {
                stream.Write(buffer, 0, bufferLength);
            }
        }

        internal string ToStringInfo()
        {
            return Encoding.UTF8.GetString(datas.Span);
        }

        public static void GetBodyLength(StringBuilder sb, int start, int length)
        {
            int end = start + length;

            //if (sb.RangeContains(Http.FieldNames.) ;

            for (int i = start; i < end; i++)
            {

            }
        }

        public static EBodyLengthDefenitionType DefineLengthType(StringBuilder sb)
        {
            throw new System.NotImplementedException();
        }

        internal void SetBodyLength(int bodyLength)
        {
            this.leftToRead = bodyLength;
            buffer = ArrayPool<byte>.Shared.Rent(bodyLength);
            datas = new Memory<byte>(buffer, 0, bodyLength);
        }

        public void AppendChunked(ReadOnlySequence<byte> seq)
        {
            int length = (int)seq.Length;
            foreach (var seg in seq)
            {
                memoryStream.Write(seg.Span);
            }
            completed += length;

            if (isRedingChunk)
            {
                leftToRead -= length;
                isRedingChunk = leftToRead > 0;
            }
        }
        public void Append(ReadOnlySequence<byte> seq)
        {
            int length = (int)seq.Length;
            seq.CopyTo(datas.Span.Slice(completed, length));
            completed += length;
            leftToRead -= length;
        }

        public void Complete()
        {
            isCompleted = true;
        }

        public void BeginChunked()
        {
            isChunked = true;
            memoryStream = new MemoryStream();
        }
        public void EndChunked()
        {
            buffer = memoryStream.ToArray();
            memoryStream.Dispose();
            memoryStream = default;
        }

        public static int FindBodyLength(in ReadOnlyMemory<byte> mem)
        {
            EnumeratorForSpanLine enumeratorForSpanNewLine = new EnumeratorForSpanLine(in mem);

            while (enumeratorForSpanNewLine.MoveNext())
            {
                var line = enumeratorForSpanNewLine.Current;
                string s = Encoding.UTF8.GetString(line.Span);
                if (s.Contains("Content-Length", StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new System.NotImplementedException();
                }
            }
            return 0;
        }
    }

    public enum EBodyLengthDefenitionType
    {
        None,
        Empty,
        TillStreamEnd,
        Chunked,
        DefinedByContentLength,
    }
}