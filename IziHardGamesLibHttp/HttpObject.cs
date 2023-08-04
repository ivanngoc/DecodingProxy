using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using IziHardGames.Libs.NonEngine.Memory;

namespace HttpDecodingProxy.ForHttp
{
    public class HttpObject : IHttpObject, IDisposable
    {
        public StringBuilder sb = new StringBuilder(8096);
        public readonly HttpBody body = new HttpBody();
        public readonly HttpFieldsV11 fields = new HttpFieldsV11();

        public HttpProxyMessage bind;

        public bool isCompleted;
        public bool isErrorReadMsg;
        public bool isReadingFields;
        public bool isReadingBody;
        public bool isReadingBodyChunked;
        private IPoolReturn<HttpObject> pool;
        public int type;
        public uint sequnce;

        public string InfoRaw => sb.ToString();

        public void BindToPool(IPoolReturn<HttpObject> pool)
        {
            this.pool = pool;
        }
        public HttpObject Bind(HttpProxyMessage message)
        {
            this.bind = message;
            return this;
        }

        public void WriteTo(Stream stream)
        {
            stream.Write(Encoding.UTF8.GetBytes(sb.ToString()).Concat(body.datas.ToArray()).ToArray());
        }
        public string ToStringInfo()
        {
            return $"type:{type} sequnce:{sequnce}" +
                $"{Environment.NewLine}" +
                $"{fields.ToStringInfo()}" +
                $"{body.ToStringInfo()}";
        }
        public void Dispose()
        {
            if (pool != null)
            {
                pool.Return(this);
                pool = default;
            }

            sb.Length = 0;
            body.Dispose();
            fields.Dispose();

            isCompleted = false;
            isErrorReadMsg = false;
            isReadingFields = false;
            isReadingBody = false;
            isReadingBodyChunked = false;
        }

        public bool IsDoubleEndLine()
        {
            if (sb.Length > 3)
            {
                int endIndex = sb.Length - 1;
                if (sb[endIndex] == '\n' && sb[endIndex - 1] == '\r' && sb[endIndex - 2] == '\n' && sb[endIndex - 3] == '\r') return true;
            }
            return false;
        }
        public ArraySegment<byte> FillFields(ArraySegment<byte> segemnt)
        {
            ArraySegment<byte> leftover = default;
            var buffer = segemnt.Array;
            var readed = segemnt.Count;

            while (true)
            {
                var res = Parse(buffer, segemnt.Offset, readed, readed);
                leftover = new ArraySegment<byte>(buffer, res.Item2, readed - res.Item2);

                if (res.Item1)
                {
                    fields.ParseLines();
                    fields.ApplyFields();
                    break;
                }
            }
            if (leftover.Count > 0)
            {
                return leftover;
            }
            else
            {

                return new ArraySegment<byte>(Array.Empty<byte>(), 0, 0);
            }
        }

        private (bool, int) Parse(byte[] buffer, int offset, int length, int readed)
        {
            if (readed == 0) return (false, 0);
            int index = Array.IndexOf<byte>(buffer, (byte)'\n', offset, readed);
            if (index < 0)
            {
                return (false, 0);
            }
            else
            {
                Span<byte> line = new Span<byte>(buffer, offset, index - offset + 1);
                fields.AddField(line);
                var res = Parse(buffer, offset + line.Length, length, readed - line.Length);

                return (line.Length == 2, line.Length + res.Item2);
            }
        }

        public ArraySegment<byte> FillBody(ArraySegment<byte> segment)
        {
            int bodyLength = fields.AnalyzBodyLength();

            if (bodyLength > 0)
            {
            }
            else if (bodyLength == -2)
            {
                body.isChunked = true;
            }
            throw new System.NotImplementedException();
        }
        public ArraySegment<byte> FillBodyChunked(ArraySegment<byte> segment)
        {
            throw new NotImplementedException();
        }
        public int FillBody(in ReadOnlySequence<byte> seq)
        {
            seq.CopyTo(body.datas.Span.Slice(body.completed));
            body.completed += (int)seq.Length;
            body.leftToRead -= (int)seq.Length;
            isCompleted = body.leftToRead == 0;
            isReadingBody = !isCompleted;
            return (int)seq.Length;
        }
        /// <summary>
        /// <see cref="HttpPipedIntermediary.AwaitBody(HttpBinary, IziHardGames.Libs.Networking.Pipelines.TcpClientPiped)"/>
        /// <see cref="HttpBody.ReadBody(Stream, HttpObject)"/>
        /// <see cref=""/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public int FillBodyChunked(in ReadOnlySequence<byte> seq)
        {
            int consumed = default;
            ReadOnlySequence<byte> buffer = seq;

            if (body.isRedingChunk)
            {
                int toConsume = body.leftToRead > seq.Length ? (int)seq.Length : body.leftToRead;
                body.AppendChunked(buffer.Slice(0, toConsume));
                buffer = buffer.Slice(toConsume);
                consumed += toConsume;
            }
            while (buffer.Length != 0 && !body.isRedingEnclosure)
            {
                SequencePosition? position = buffer.PositionOf((byte)'\n');
                int consumedLoop = 0;

                if (position != null)
                {
                    var nextPos = buffer.GetPosition(1, position.Value);
                    var slice = buffer.Slice(0, nextPos);
                    body.AppendChunked(slice);

                    int lengthControl = (int)slice.Length;
                    if (lengthControl == 2)
                    {
                        consumed += lengthControl;
                        buffer = buffer.Slice(lengthControl);
                        continue;
                    }
                    consumedLoop += lengthControl;
                    int length = slice.ParseHex();
                    int leftToRead = (int)seq.Length - consumed - consumedLoop;

                    if (length > leftToRead)
                    {
                        body.isRedingChunk = true;
                        body.leftToRead = length;
                        length = leftToRead;
                    }

                    consumedLoop += length;

                    consumed += consumedLoop;
                    body.AppendChunked(buffer.Slice(slice.Length, length));
                    buffer = buffer.Slice(consumedLoop);
                    if (length == 0)
                    {
                        body.isRedingEnclosure = true;
                        break;
                    }
                }
            }
            if (body.isRedingEnclosure)
            {
                body.AppendChunked(seq.Slice(0, 2));
                body.EndChunked();
                body.Complete();
                isReadingBodyChunked = false;
                isCompleted = true;
                consumed += 2;
                return consumed;
            }
            return consumed;
        }

        public void BeginMsg()
        {
            isReadingFields = true;
        }

        public int Push(ReadOnlySequence<byte> buffer)
        {
            int consumed = default;

            if (isReadingFields)
            {
                consumed += ParseLines(in buffer);
                buffer = buffer.Slice(consumed);
            }
            if (isReadingBody)
            {
                consumed += FillBody(in buffer);
            }
            else if (isReadingBodyChunked)
            {
                consumed += FillBodyChunked(in buffer);
            }
            return consumed;
        }
        /// <summary>
        /// <see cref="HttpPipedIntermediary.ParseLineRecursive(HttpBinary, ReadOnlySequence{byte}, int)"/>
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private int ParseLines(in ReadOnlySequence<byte> buffer)
        {
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position != null)
            {
                var item = fields;
                var nextPos = buffer.GetPosition(1, position.Value);
                var slice = buffer.Slice(0, nextPos);
                var span = item.AddField(slice);
                ReadOnlySequence<byte> left = buffer.Slice(nextPos);
                // if empty line
                if (span.Length == 2) // && span[0] == '\r' && span[1] == '\n')  // almost impossible case
                {
                    isReadingFields = false;
                    fields.ParseLines();
                    fields.ApplyFields();
                    int bodyLength = fields.AnalyzBodyLength();
                    if (bodyLength > 0)
                    {
                        isReadingBody = true;
                        body.SetBodyLength(bodyLength);
                    }
                    else if (bodyLength == -2)
                    {
                        isReadingBodyChunked = true;
                        body.BeginChunked();
                    }
                    else isCompleted = true;
                    return 2;
                }
                var consumed = ParseLines(in left);
                // there might be unicode support in fields turned on. therefore each symbol might be more than 1 byte
                return consumed + (int)slice.Length;
            }
            return 0;
        }
    }
}