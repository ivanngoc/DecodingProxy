using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Libs.Pipelines;
using IziHardGames.Proxy.Sniffing.ForHttp;

namespace IziHardGames.Libs.ForHttp.Http11
{

    public class ObjectReaderHttp11Piped : PipedObjectReader<HttpReadResult>, IDisposable
    {
        private IReader? reader;
        private int type;
        private CancellationTokenSource? cts;
        private IPoolObjects<HttpBinaryMapped>? pool;

        public int Type => type;

        public void Initilize(IReader reader, int type, CancellationTokenSource cts, IPoolObjects<HttpBinaryMapped> pool)
        {
#if DEBUG
            if (reader == null) throw new NullReferenceException();
#endif
            this.reader = reader;
            this.type = type;
            this.cts = cts;
            this.pool = pool;
        }

        public override async ValueTask<HttpReadResult> ReadObjectAsync()
        {
            HttpBinaryMapped item = pool!.Rent().Init();
            item.BindToPool(pool, type);

            if (!cts!.IsCancellationRequested)
            {
                await AwaitFeilds(item, cts.Token).ConfigureAwait(false);
#if DEBUG
                Console.WriteLine(item.ToStringFields());
#endif
                await AwaitBody(item, cts.Token).ConfigureAwait(false);
            }
            return new HttpReadResult(item, HttpReadResult.STATUS_COMPLETE);
        }

        private async Task AwaitFeilds(HttpBinaryMapped item, CancellationToken token)
        {
            var reader = this.reader;
            item.FieldsStart();
            int offset = 0;
            bool isEnd = default;
            do
            {
                ReadResult result = await reader!.ReadPipeAsync(token).ConfigureAwait(false);
                var buffer = result.Buffer;
                if (buffer.Length > 0)
                {
                    isEnd = ParseLineRecursive(item, ref buffer, offset);
                    offset = item.Length;
                }
                else
                {
                    Logger.LogWarning($"{nameof(ObjectReaderHttp11Piped)}.{AwaitFeilds} Null read Not implemented");
                }
                reader.ReportConsume(buffer.Start);
            } while (!isEnd);
        }

        // исхлдим их того что строка короткая поэтому если к уже считанному сегменту прибавится еще сегмент
        // то не надо оптимизировать двойное чтение в случае если в сегменте не будет найден конец строки
        // поэтому даже если будет найдено 10 строк и не будет найден с одного чтения \r\n\r\n то издержки производительности будут ничтожеными исходя из количества таких случаев(0,01%)
        /// <summary>
        /// <see cref="HttpObject.Push(ReadOnlySequence{byte})"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns>
        /// <see cref="bool"/> - fields enclosure finded (true)<br/>
        /// <see cref="ReadOnlySequence{T}"/> - latest buffer after last succesfull slicing<br/>
        /// </returns>
        private bool ParseLineRecursive(HttpBinaryMapped item, ref ReadOnlySequence<byte> buffer, int offset)
        {
#if DEBUG
            var s = buffer.ToStringUtf8();
#endif            
            SequencePosition? position = buffer.PositionOf((byte)'\n');

            if (position != null)
            {
                int index = item.FieldsCount;
                var nextPos = buffer.GetPosition(1, position.Value);
                var slice = buffer.Slice(0, nextPos);
                var span = item.AddField(slice, offset);
                buffer = buffer.Slice(nextPos);
                // if empty line
                if (span.Length == ConstantsForHttp.LENGTH_LF) // && span[0] == '\r' && span[1] == '\n')  // almost impossible case
                {
                    item.AllocateFieldsMap();
                    item.MapField(index, offset, ConstantsForHttp.LENGTH_LF);
                    item.FieldsEnd();
                    return true;
                }
                // Skip the line + the \n character (basically position)
                var isEnd = ParseLineRecursive(item, ref buffer, offset + span.Length);
                //if (consumed.Item2 > 0) httpObject.MapField(index, offset, span.Length);  // in case of partial reading in few goes
                item.MapField(index, offset, span.Length);
                return isEnd;
            }
            item.AllocateFieldsMap();
            return false;
        }

        /// <summary>
        /// <see cref="HttpObject.FillBodyChunked(in ReadOnlySequence{byte})"/>
        /// <see cref="HttpBody.ReadBody(Stream, HttpObject)"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private async Task AwaitBody(HttpBinaryMapped item, CancellationToken token)
        {
            item.BodyStart();
            int length = item.GetBodyLength();
            var reader = this.reader;
            //ReadResult resultD = await reader.ReadAsync().ConfigureAwait(false);
            //var bufferD = resultD.Buffer;
            //string debug = Encoding.UTF8.GetString(bufferD);

            if (length > 0)
            {
                item.EnsureCapacity(length);

                while (length > 0)
                {
                    ReadResult result = await reader!.ReadPipeAsync(token).ConfigureAwait(false);

                    var buffer = result.Buffer;

                    if (length > buffer.Length)
                    {
                        length -= (int)buffer.Length;
                        item.AddBody(buffer);
                        reader.ReportConsume(buffer.End);
                    }
                    else
                    {
                        SequencePosition position = buffer.GetPosition(length);
                        var slice = buffer.Slice(0, length);
                        item.AddBody(slice);
                        reader.ReportConsume(position);
                        item.BodyEnd();
                        goto END;
                    }
                }
            }
            else if (length == -2)
            {
                await ReadBodyChunkedV1(reader!, item, token).ConfigureAwait(false);

            }
            else if (length == -3)
            {
                item.isReadingUntilCloseConnection = true;
                item.BodyEnd();
                goto END;
            }
            END: { }
        }

        private async Task ReadBodyChunkedV1<T>(T reader, HttpBinaryMapped item, CancellationToken token) where T : IReader
        {
            SequencePosition? pos;
            SequencePosition position;
            bool isReadingChunk = false;
            bool isReadingControl = true;
            bool isReadingEnclosure = false;
            bool isEnd = false;
            int lengthLeft = 0;

            while (true)
            {
                var result = await reader.ReadPipeAsync(token).ConfigureAwait(false);
                var buffer = result.Buffer;
                int lengthLoop = buffer.Length < lengthLeft ? (int)buffer.Length : lengthLeft;
                if (isReadingControl)
                {   // search control line ending
                    pos = buffer.PositionOf((byte)'\n');

                    if (pos == null)
                    {
                        continue;
                    }
                    isReadingControl = false;
                    var nextPos = buffer.GetPosition(1, pos.Value);
                    var controline = buffer.Slice(0, nextPos);
                    // add control line to container
                    item.AddBodyForSure(controline);
                    buffer = buffer.Slice(controline.Length);
                    int chunkLength = controline.ParseHex();
                    if (chunkLength == 0)
                    {
                        isReadingEnclosure = true;
                        isEnd = true;
                        item.BodyEnd();
                        goto END;
                    }
                    else
                    {
                        isReadingChunk = true;
                        lengthLeft = chunkLength;
                        lengthLoop = chunkLength > buffer.Length ? (int)buffer.Length : chunkLength;
                    }
                }

                if (isReadingChunk)
                {
                    var sliceChunk = buffer.Slice(0, lengthLoop);
                    // add chunked body to container
                    item.AddBodyForSure(sliceChunk);
                    buffer = buffer.Slice(lengthLoop);
                    lengthLeft -= lengthLoop;
                    if (lengthLeft == 0) isReadingEnclosure = true;
                    isReadingChunk = !isReadingEnclosure;
                }
                END:
                if (isReadingEnclosure)
                {
                    // read CRLF after chunk and add to container
                    var lf = buffer.Slice(0, ConstantsForHttp.LENGTH_LF);
                    item.AddBodyForSure(lf);
                    buffer = buffer.Slice(ConstantsForHttp.LENGTH_LF);
                    isReadingControl = true;
                    isReadingEnclosure = false;
                    if (isEnd)
                    {
                        position = buffer.GetPosition(0);
                        reader.ReportConsume(position);
                        break;
                    }
                }
                position = buffer.GetPosition(0);
                reader.ReportConsume(position);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            reader = default;
            type = default;
            cts = default;
            pool = default;
        }
    }
}