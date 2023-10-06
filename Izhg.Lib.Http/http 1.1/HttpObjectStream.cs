using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Collections;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.Streaming;
using IziHardGames.Libs.Text;
using ProxyLibs.Extensions;
using Query = IziHardGames.Libs.HttpCommon.Streaming.HttpQuery<IziHardGames.Libs.HttpCommon.Streaming.WrapIndexerForStringBuilder>;
using Wrap = IziHardGames.Libs.HttpCommon.Streaming.WrapIndexerForStringBuilder;



namespace IziHardGames.Proxy.Sniffing.ForHttp
{

    internal delegate void HandlerForBufferReader(byte[] buffer, ref int start, ref int length);

    /// <summary>
    /// Stream для записи данных в массив байтов и интерпретации данных по стандарту http
    /// </summary>
    public class HttpObjectStream : ReusableStream
    {
        public const int DEFAULT_BUFFER_SIZE = 1 << 20;
        /// <summary>
        /// <see cref="ReadHeaders"/>
        /// </summary>
        private const int ACTION_READ_HEADERS = 0;
        /// <summary>
        /// <see cref="ReadBody(byte[], ref int, ref int)"/>
        /// </summary>
        private const int ACTION_READ_BODY = 1;
        private const int ACTION_READ_CHUNK = 2;

        private readonly HandlerForBufferReader[] readers;
        private int currentReadAction;

        private SubstringMatchQueue substringQueue = new SubstringMatchQueue("\r\n\r\n");

        #region Header
        private StringBuilder header = new StringBuilder();
        /// <summary>
        /// Position after \r\n\r\n
        /// </summary>
        private int positionHeadersEnd;
        private int headersLength;
        #endregion

        private int offset;
        private int length;
        private int positionRead;
        private int positionWrite;

        private byte[] buffer;

        private Action<HttpObject> resultReciever;
        private Queue<HttpObject> queue;
        private HttpObject current;

        /// <summary>
        /// <see langword="true"/> - request
        /// <see langword="false"/> - response
        /// </summary>
        private bool isRequest;

        /// <summary>
        /// next position after  last known \r\n
        /// </summary>
        private int indexLastLineEnd;


        private int bodyLength;
        private int bodyLengthLeft;

        private bool isReadingFields;
        private bool isReadingBody;
        private bool isReadHeadersEnd;
        private bool isReadingChunkSize;
        private bool isReadingChunkData;
        private bool isReadingUntilSteamEnd;

        public override bool CanRead { get => throw new System.NotSupportedException(); }
        public override bool CanSeek { get => throw new System.NotSupportedException(); }
        public override bool CanWrite { get => throw new System.NotSupportedException(); }
        public override long Length { get => length; }
        public override long Position { get => positionRead; set => positionRead = (int)value; }

        protected int FreeSpace { get => buffer.Length - (int)Length; }

        public HttpObjectStream() : base()
        {
            readers = new HandlerForBufferReader[]
          {
                ReadHeaders,
                ReadBody,
                AdditiveReadChunkSize,
                AdditiveReadChunkData,
          };
        }

        public HttpObjectStream Init(int capacity = DEFAULT_BUFFER_SIZE)
        {
            buffer = ArrayPool<byte>.Shared.Rent(capacity);
            return this;
        }
               
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (length > 0)
            {
                int proceed = length > count ? count : length;
                Array.Copy(this.buffer, this.positionRead, buffer, offset, proceed);
                var val = length - proceed;
                Interlocked.Exchange(ref this.length, val);
                this.positionRead += proceed;
                return proceed;
            }
            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }


        public override void Write(byte[] buffer, int offset, int count)
        {
            Array.Copy(buffer, offset, this.buffer, positionWrite, count);
            positionWrite += count;
            int val = length + count;
            Interlocked.Exchange(ref this.length, val);
        }

        public void EnsureCapacity(int toWrite)
        {
            int target = positionWrite + toWrite;
            if (buffer.Length < (target))
            {
                Resize(target);
            }
        }

        private void Resize(int target)
        {
            ArrayPool<byte>.Shared.Rent(target);
        }

        public static HttpObjectStream Rent()
        {
            return PoolObjectsConcurent<HttpObjectStream>.Shared.Rent();
        }

        public static void Return(HttpObjectStream stream)
        {
            PoolObjectsConcurent<HttpObjectStream>.Shared.Return(stream);
        }


        public void StartReusable(Action<HttpObject> resultReciever, bool isRequest)
        {
            this.resultReciever = resultReciever;
            this.isRequest = isRequest;

            this.positionHeadersEnd = -1;
            this.indexLastLineEnd = -1;
            this.positionRead = default;
            this.bodyLength = default;
            this.headersLength = default;

            this.isReadingChunkSize = false;
            this.isReadingBody = false;
            this.isReadHeadersEnd = false;

            this.currentReadAction = ACTION_READ_HEADERS;
        }

        private void AdditiveCopyToBuffer(byte[] buffer, int offset, int count)
        {
            if (FreeSpace >= count)
            {
                Array.Copy(buffer, offset, this.buffer, default, count);
            }
            else
            {
                Extend(count);
            }
            throw new System.NotImplementedException();
        }

        private void Extend(int count)
        {
            throw new System.NotImplementedException();
        }

        #region Readers
        private void ReadHeaders(byte[] buffer, ref int indexStart, ref int length)
        {
            int end = indexStart + length;
            int lengthToCopy = length;

            for (int i = indexStart; i < end; i++)
            {
                if (substringQueue.IsMatchOnEnqeue(buffer[i]))
                {
                    this.isReadHeadersEnd = true;
                    this.positionHeadersEnd = i + 1;
                    lengthToCopy = this.positionHeadersEnd - indexStart;
                    break;
                }
            }
            AdditiveCopyToBuffer(buffer, indexStart, lengthToCopy);
            length -= lengthToCopy;
            indexStart = this.positionHeadersEnd;

            if (isReadHeadersEnd)
            {
                MapHeaders();
            }
            throw new System.NotImplementedException();
        }

        private void ReadBody(byte[] buffer, ref int offset, ref int count)
        {
            int lengthToCopy = count <= bodyLengthLeft ? count : bodyLengthLeft;
            bodyLengthLeft -= lengthToCopy;
            AdditiveCopyToBuffer(buffer, offset, lengthToCopy);
            count -= lengthToCopy;
            offset += lengthToCopy;

            if (bodyLengthLeft == 0)
            {
                FormMsg();
            }
        }

        private void AdditiveReadChunkSize(byte[] buffer, ref int indexStart, ref int length)
        {
            int end = indexStart + length;
            Span<char> hex = stackalloc char[ConstantsForHttp.MAX_INT_CHARS];
            int count = default;

            // read chunk line untill CRLF
            int crlf = StringHelper.FindIndexAfterNewLine(buffer, indexStart);

            for (int i = indexStart; i < end; i++)
            {
                char c = (char)buffer[i];

                if (Uri.IsHexDigit(c))
                {
                    hex[count] = c;
                    count++;
                }
                else
                {
                    break;
                }
            }
            int size = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            this.bodyLengthLeft = size;
        }

        private void AdditiveReadChunkData(byte[] buffer, ref int indexStart, ref int length)
        {
            throw new System.NotImplementedException();
        }
        #endregion

        private void InterpretNextFieldLine(int indexLineEnd)
        {
            int indexLineStart = this.indexLastLineEnd;

            if (indexLineStart < 0)
            {
                indexLineStart = 0;
            }

            int currentLineLength = indexLineEnd - indexLineStart + 1;

            if (currentLineLength > 0)
            {
                Query.From(new Wrap(header, 0, positionHeadersEnd)).Header(ConstantsForHttp.FieldSubstrings.FIELD_TRANSFER_ENCODING).Values.Last().Is(ConstantsForHttp.FieldValues.VALUE_CHUNKED);

                if (header.RangeContains(indexLineStart, currentLineLength, ConstantsForHttp.FieldSubstrings.FIELD_TRANSFER_ENCODING))
                {

                }
                else if (header.RangeContains(indexLineStart, currentLineLength, ConstantsForHttp.FieldSubstrings.SUBSTRING_CONTENT_LENGTH))
                {

                }
            }
            else
            {
                positionHeadersEnd = indexLineEnd + 2;
            }
        }

        private void FormMsg()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// <see cref="HttpBody.ReadBody(Stream, HttpObject)"/>
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        private void MapHeaders()
        {
            int offset = default;

            var fields = Query.From(new Wrap(header, offset, this.length));
            fields.Calculate();

            bool isContentTransferChunked = default;
            bool isContentLength = default;
            bool isNoBody = default;

            var startLine = fields.StartLine;

            if (isRequest)
            {

            }

            foreach (var field in fields)
            {
                string fieldName = field.GetName();

                switch (fieldName)
                {
                    case ConstantsForHttp.FieldSubstrings.FIELD_TRANSFER_ENCODING:
                        {
                            if (field.Values.Last().Is(ConstantsForHttp.FieldValues.VALUE_CHUNKED))
                            {

                            }
                            break;
                        }
                    case ConstantsForHttp.FieldSubstrings.SUBSTRING_CONTENT_LENGTH:
                        {
                            break;
                        }

                    default: break;
                }
            }

            if (!isNoBody)
            {
                if (isContentTransferChunked)
                {

                }
                else if (isContentLength)
                {

                }
                else
                {
                    throw new System.NotImplementedException("Body can't be determined");
                }
            }
            throw new System.NotImplementedException();
        }
        internal void CheckCompletion()
        {
            throw new NotImplementedException();
        }

        public static int GetContentLength(StringBuilder sb, int start, int length)
        {
            int end = start + length;
            Span<char> span = stackalloc char[ConstantsForHttp.MAX_INT_CHARS];
            int indexStart = sb.IndexAfter(start, length, ConstantsForHttp.FieldSubstrings.SUBSTRING_CONTENT_LENGTH);

            if (indexStart < 0) throw new ArgumentOutOfRangeException($"Substring {ConstantsForHttp.FieldSubstrings.SUBSTRING_CONTENT_LENGTH} Not Founded in range start:{start} length:{length}");

            int count = default;

            for (int i = indexStart; i < end; i++)
            {
                if (!char.IsDigit(sb[i])) break;

                span[count] = sb[i];
                count++;
            }

            int bodyLength = int.Parse(span.Slice(0, count));
            return bodyLength;
        }

        public bool TryAdvance(out HttpObject obj)
        {
            throw new NotImplementedException();
        }

        public void TryDispose()
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            isReadingFields = default;
            positionRead = default;
            positionWrite = default;
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = null;
            }
        }

        public void WriteAdvance(byte[] buffer, int offset, int length)
        {
            Write(buffer, offset, length);
            Advance(length);
        }

        private void Advance(int length)
        {
            if (current == null)
            {
                current = PoolObjectsConcurent<HttpObject>.Shared.Rent();
                isReadingFields = true;
            }

            var obj = current;

            if (isReadingFields)
            {
                int count = default;

                for (int i = offset; i < offset + length; i++)
                {
                    obj.sb.Append((char)buffer[i]);

                    if (obj.IsDoubleEndLine())
                    {
                        throw new System.NotImplementedException();
                        isReadingFields = false;
                        //obj.fields.ReadFields();
                        //obj.fields.ParseLines();
                        //obj.fields.ApplyFields();
                        break;
                    }
                    else
                    {
                        count++;
                    }
                }
                offset += count;
            }
            else if (isReadingBody)
            {

            }
            else
            {
                throw new System.InvalidOperationException("Reading status must be defined");
            }
        }

        public bool TryPeek(out HttpObject obj)
        {
            throw new NotImplementedException();
        }
    }
}