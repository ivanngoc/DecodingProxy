using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IziHardGames.Libs.ForHttp;
using IziHardGames.Libs.IO;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.Pipelines.Contracts;
using IziHardGames.Libs.NonEngine.Enumerators;
using IziHardGames.Libs.NonEngine.Memory;

namespace HttpDecodingProxy.ForHttp
{
    public class HttpBinaryMapped : HttpBinary, IHttpObject, IDisposable, ICloneable, IPoolBind<HttpBinaryMapped>
    {
        private int lengthAnalyzed;
        public int offset;
        public int indexEndFields;
        /// <summary>
        /// Где кончается тело
        /// </summary>
        public int indexEndBody;
        private (int, int)[] maps;
        private int fieldsCount;
        private int fieldsCountMapped;
        private IPoolReturn<HttpBinaryMapped>? pool;
        public int type;
        public string debug;
        public bool isReadingUntilCloseConnection;
        private int generation;
        private int deaths;

        public int FieldsCount => fieldsCount;
        public int FieldsCountMapped => fieldsCountMapped;
        public int LengthBody => indexEndBody - indexEndFields;
        public int Length => length;
        /// <summary>
        /// Indluding enclosing \r\n
        /// </summary>
        public int LengthHeaders => indexEndFields - offset;
        public string Fields => ToStringFieldsFromMap();
        public string Raw => ToString();
        public bool IsCloseRequired => CheckCloseConnection();
        public ReadOnlyMemory<byte> Memory => new Memory<byte>(datas, offset, length);
        public ReadOnlySpan<byte> Span => Memory.Span;
        public ReadOnlyMemory<byte> this[string fieldName] => GetFieldValue(fieldName);
        public ReadOnlySpan<byte> this[int index] => GetLineByIndex(index);

        public HttpBinaryMapped()
        {
            datas = Array.Empty<byte>();
            //buffer = new byte[(1 << 20) * 16];
            maps = Array.Empty<(int, int)>();
        }
        public HttpBinaryMapped(int size)
        {
            datas = ArrayPool<byte>.Shared.Rent(size);
            maps = Array.Empty<(int, int)>();
        }

        public HttpBinaryMapped Init()
        {
            generation++;
            return this;
        }
        public override void Dispose()
        {
            base.Dispose();
            deaths++;

            if (maps.Length > 0) ArrayPool<(int, int)>.Shared.Return(maps);
            maps = Array.Empty<(int, int)>();
            if (pool != null) pool.Return(this);
            pool = default;

            fieldsCount = default;
            fieldsCountMapped = default;
            lengthAnalyzed = default;
            offset = default;
            indexEndBody = default;
            indexEndFields = default;
            type = default;
            isReadingUntilCloseConnection = default;
        }

        public int EnsureCapacity(int toAdd)
        {
            int targetSize = toAdd + this.length;
            if (datas.Length < targetSize)
            {
                var newarr = ArrayPool<byte>.Shared.Rent(targetSize);
                if (this.length > 0) Array.Copy(datas, 0, newarr, 0, this.length);
                if (datas.Length != 0) ArrayPool<byte>.Shared.Return(datas);
                datas = newarr;
            }
            return targetSize;
        }

        
        #region Getters
        private ReadOnlyMemory<byte> GetFieldValue(string fieldName)
        {
            throw new NotImplementedException();
        }
        private ReadOnlySpan<byte> GetLineByIndex(int index)
        {
            return new ReadOnlySpan<byte>(datas, maps[index].Item1, maps[index].Item2);
        }
        public ReadOnlyMemory<byte> GetMemory()
        {
            return new ReadOnlyMemory<byte>(datas, offset, length);
        }
        /// <summary>
        /// <see cref="HttpBody.ReadBody(Stream, HttpObject)"/>
        /// </summary>
        /// <returns></returns>
        public int GetBodyLength()
        {
            if (CheckNoBody())
            {
                lengthAnalyzed = 0;
                return 0;
            }
            if (CheckChunked())
            {
                lengthAnalyzed = -2;
                return -2;
            }
            else if (TryGetFieldValueAsInt(HttpLibConstants.FieldNames.NAME_CONTENT_LENGTH, out int val))
            {
                lengthAnalyzed = val;
                return val;
            }
            lengthAnalyzed = -3;
            return -3;
        }

        public (string, int) FindHostAndPortFromField()
        {
            if (TryGetFieldValueCI(HttpLibConstants.FieldNames.NAME_HOST, out var value))
            {
#if DEBUG
                var s = value.ToStringUtf8();
#endif
                int indexDelimter = value.IndexOf((byte)':');
                if (indexDelimter < 0) return (Encoding.UTF8.GetString(value), 80);
                var portSlice = value.Slice(indexDelimter + 1, value.Length - indexDelimter - 1);
                Span<char> portChars = stackalloc char[portSlice.Length];
                portSlice.CopyAsChar(ref portChars);
                var sliceHost = value.Slice(0, indexDelimter);
#if DEBUG
                var s1 = sliceHost.ToStringUtf8();
                var s2 = portSlice.ToStringUtf8();
#endif
                var host = Encoding.UTF8.GetString(sliceHost);
                var port = int.Parse(portChars);
                return (host, port);
            }
            throw new InvalidOperationException("This is field must be in request");
        }
        public EnumeratorArrayRef<(int, int)> GetEnumerator()
        {
            return new EnumeratorArrayRef<(int, int)>(maps, fieldsCount);
        }

        public bool TryGetFieldValueCI(string fieldName, out ReadOnlySpan<byte> bytes)
        {
            for (int i = 0; i < fieldsCountMapped; i++)
            {
                var span = new Span<byte>(datas, maps[i].Item1, maps[i].Item2);
                if (span.Length < fieldName.Length) continue;
                for (int j = 0; j < fieldName.Length; j++)
                {
                    if (char.ToLowerInvariant((char)span[j]) != char.ToLowerInvariant((char)fieldName[j])) goto NEXT;
                }
                int offset = fieldName.Length + HttpLibConstants.LENGTH_LF;  // exclude substring: field-name:[BWS] .* \r\n
                int count = span.Length - (fieldName.Length + 4);
                if (count > 0)
                {
                    bytes = new Span<byte>(datas, maps[i].Item1 + offset, count);
                    return true;
                }
                NEXT: continue;
            }
            bytes = default;
            return false;
        }
        public bool TryGetFieldValueAsInt(string fieldName, out int val)
        {
            for (int i = 0; i < fieldsCount; i++)
            {
                var span = new Span<byte>(datas, maps[i].Item1, maps[i].Item2);
                if (span.Length < fieldName.Length) continue;
                for (int j = 0; j < fieldName.Length; j++)
                {
                    if (span[j] != fieldName[j]) goto NEXT;
                }
                int offset = fieldName.Length + 2;  // exclude substring: field-name:[BWS] .* \r\n
                int count = span.Length - (fieldName.Length + 4);
                if (count > 0)
                {
                    Span<char> value = stackalloc char[count];

                    for (int k = 0; k < count; k++)
                    {
                        value[k] = (char)span[offset + k];
                    }
                    if (int.TryParse(value, out val))
                    {
                        return true;
                    }
                }
                NEXT: continue;
            }
            val = -1;
            return false;
        }

        #endregion

        public Span<byte> AddField(ReadOnlySequence<byte> readOnlySequence, int offset)
        {
            this.fieldsCount++;
            int sizeToAdd = (int)readOnlySequence.Length;
            this.length = EnsureCapacity(sizeToAdd);
            Span<byte> segment = new Span<byte>(datas, offset, sizeToAdd);
            readOnlySequence.CopyToSafe(segment);
            return segment;
        }

        public void AddBody(ReadOnlySequence<byte> readOnlySequence)
        {
            int lengthToCopy = (int)readOnlySequence.Length;
            readOnlySequence.CopyToSafe(new Span<byte>(datas, this.length, lengthToCopy));
            this.length += lengthToCopy;
        }
        public void AddBodyForSure(ReadOnlySequence<byte> readOnlySequence)
        {
            var offset = this.length;
            this.length = EnsureCapacity((int)readOnlySequence.Length);
            int lengthToCopy = (int)readOnlySequence.Length;
            readOnlySequence.CopyToSafe(new Span<byte>(datas, offset, lengthToCopy));
        }

        public void BodyStart()
        {

        }
        public void BodyEnd()
        {
            debug = Encoding.UTF8.GetString(GetMemory().Span);
            indexEndBody = length;
        }
        internal void AllocateFieldsMap()
        {
            if (maps.Length < fieldsCount)
            {
                var newMap = ArrayPool<(int, int)>.Shared.Rent(fieldsCount);
                if (maps.Length > 0)
                {
                    Array.Copy(maps, 0, newMap, 0, fieldsCountMapped);
                    ArrayPool<(int, int)>.Shared.Return(maps);
                }
                maps = newMap;
            }
        }

        internal void MapField(int index, int offset, int length)
        {
            maps[index] = (offset, length);
            fieldsCountMapped++;
        }

        public string ToStringFields()
        {
            return Encoding.UTF8.GetString(new Memory<byte>(datas, offset, indexEndFields).Span);
        }
        public string ToStringFieldsFromMap()
        {
            return maps.Take(fieldsCountMapped)
                  .Select(x => new Memory<byte>(datas, x.Item1, x.Item2))
                  .Select(y => Encoding.UTF8.GetString(y.Span))
                  .Aggregate((x, y) => x + y);
        }

        /// <summary>
        /// <see cref="HttpFieldsV11.IsChunkedLast"/>
        /// </summary>
        /// <returns></returns>
        public bool CheckChunked()
        {
            if (TryGetFieldValueCI(HttpLibConstants.FieldNames.NAME_TRANSFER_ENCODING, out ReadOnlySpan<byte> span))
            {
                return span.GotSubsequenceProbablyAtBackCI(HttpLibConstants.FieldValues.VALUE_CHUNKED);
            }
            return false;
        }

        public bool CheckNoBody()
        {
            if (HttpLibConstants.TYPE_RESPONSE == type)
            {
                var status = GetStatus();
                // any 1xx
                return status < 200 || status == 204 || status == 304;
            }
            return false;
        }

        public bool TryGetStatus(out int value)
        {
            if (type == HttpLibConstants.TYPE_RESPONSE)
            {
                value = GetStatus();
                return true;
            }
            value = 0;
            return false;
        }

        public int GetStatus()
        {
            // status-line = HTTP-version SP status-code SP [ reason-phrase ]
            // SP (space)
            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(datas, maps[0].Item1, maps[0].Item2);
            int count = 0;
            Span<int> values = stackalloc int[2];

            for (int i = 0; count < 2; i++)
            {
                if (span[i] == ConstantsUtf8.SPACE)
                {
                    values[count] = i;
                    count++;
                }
            }
            span = span.Slice(values[0], values[1] - values[0]);
            return span.ParseToInt32();
        }
        private bool CheckCloseConnection()
        {
            if (TryGetFieldValueCI(HttpLibConstants.FieldNames.NAME_CONNECTION, out var raw))
            {
                return raw.GotSubsequenceProbablyAtBackCI(HttpLibConstants.FieldValues.VALUE_CLOSE_CONNECTION);
            }
            return false;
        }
        internal void BindToPool(IPoolObjects<HttpBinaryMapped> pool, int type)
        {
            BindToPool(pool);
            this.type = type;
        }


        public string ToStringHex()
        {
            return GetMemory().Span.ToStringHex();
        }
        public override string ToString()
        {
            return Encoding.UTF8.GetString(GetMemory().Span);
        }
        public string ToStringInfo()
        {
            return Encoding.UTF8.GetString(GetMemory().Span);
        }

        internal void FieldsStart()
        {
            length = 0;
        }

        internal void FieldsEnd()
        {
            indexEndFields = length;
            debug = Encoding.UTF8.GetString(GetMemory().Span);
        }

        internal async Task<bool> TryReadBodyUntilEnd<T>(T client, CancellationTokenSource cts) where T : IReader, ICheckConnection
        {
            if (isReadingUntilCloseConnection)
            {
                await ReadBodyUntilEnd(client).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        public async Task ReadBodyUntilEnd<T>(T client) where T : IReader, ICheckConnection
        {
            while (client.CheckConnectIndirectly())
            {
                var result = await client.ReadAsync();
                AddBodyForSure(result.Buffer);
                client.ReportConsume(result.Buffer.End);
            }
        }

        /// <summary>
        /// Apply fields to control connection and so on
        /// </summary>
        public void ApplyControls<T>(T pipedTcpClient) where T : IApplyControl
        {
            var version = FindVersion();

            if (version == EHttpVersion.Version11)
            {
                // keep-alive
                if (TryGetKeepAlive(out int timeout, out int max))
                {
                    int value = timeout * 1000;
                    pipedTcpClient.SetTimeouts(value, value);
                    pipedTcpClient.SetLife(max);
                }
            }
        }

        public bool TryGetKeepAlive(out int timeout, out int max)
        {
            timeout = default;
            max = default;

            if (TryGetFieldValueCI(HttpLibConstants.FieldNames.NAME_KEEP_ALIVE, out ReadOnlySpan<byte> span))
            {
                int index = 0;
                int offset = 0;
                int length = 0;
                bool isKeepItterate = false;
                bool isTimeout = false;
                bool isMax = false;
                bool result;
                do
                {
                    int start = offset;
                    isKeepItterate = span.TryFindSplit(ConstantsUtf8.COMMA, ref index, ref offset, ref length);
                    ReadOnlySpan<byte> slice = span.Slice(start, length);
                    int indexTimeout = slice.EndOfSubstringCI("timeout=");
                    if (indexTimeout > 0)
                    {
                        ReadOnlySpan<byte> value = slice.Slice(indexTimeout + 1);
                        timeout = value.ParseToInt32();
                        isTimeout = true;
                    }
                    else
                    {
                        int indexMax = slice.EndOfSubstringCI("max=");
                        if (indexMax > 0)
                        {
                            ReadOnlySpan<byte> value = slice.Slice(indexMax + 1);
                            max = value.ParseToInt32();
                            isMax = true;
                        }
                    }
                }
                while (isKeepItterate || !(result = (isTimeout && isMax)));
                return result;
            }
            return false;
        }
        public string GetVersionString()
        {
            switch (FindVersion())
            {
                case EHttpVersion.None: throw new ArgumentOutOfRangeException();
                case EHttpVersion.Version10: return HttpLibConstants.version10;
                case EHttpVersion.Version11: return HttpLibConstants.version11;
                case EHttpVersion.Version20: return HttpLibConstants.version20;
                case EHttpVersion.Version30: return HttpLibConstants.version30;
                default: goto case EHttpVersion.None;
            }
        }
        public EHttpVersion FindVersion()
        {
            ReadOnlySpan<byte> version;
            var span = this[0];

            if (type == HttpLibConstants.TYPE_REQUEST)
            {   //   get start-line
                version = span.FindSplit(ConstantsUtf8.SPACE, 2);
            }
            else
            {
                version = span.FindSplit(ConstantsUtf8.SPACE, 0);
            }
            if (version.GotSubsequenceProbablyAtBackCI(HttpLibConstants.version11)) return EHttpVersion.Version11;
            if (version.GotSubsequenceProbablyAtBackCI(HttpLibConstants.version20)) return EHttpVersion.Version20;
            if (version.GotSubsequenceProbablyAtBackCI(HttpLibConstants.version30)) return EHttpVersion.Version30;
            if (version.GotSubsequenceProbablyAtBackCI(HttpLibConstants.version10)) return EHttpVersion.Version11;
            throw new ArgumentException($"Can't parse version. Recived:{version.ToStringUtf8()}");
        }
        public EHttpMethod FindMethod()
        {
            ReadOnlySpan<byte> method;
            var span = this[0];
            if (type == HttpLibConstants.TYPE_REQUEST)
            {   //   get start-line
                method = span.FindSplit(ConstantsUtf8.SPACE, 0);
                ///<see cref="HttpMethod"/>
                if (method[0] == 'G') return EHttpMethod.GET;
                if (method[0] == 'C') return EHttpMethod.CONNECT;
                if (method[0] == 'P' && method[1] == 'U') return EHttpMethod.PUT;
                if (method[0] == 'P' && method[1] == 'O') return EHttpMethod.POST;
                return EHttpMethod.NOT_IMPLEMENTED;
            }
            else
            {
                throw new InvalidOperationException($"Method Is Available Only for Request type");
            }
        }

        public bool Validate()
        {
            var v = FindVersion();
            return true;
        }

        public object Clone()
        {
            return new HttpBinaryMapped()
            {
                datas = datas.ToArray(),
                maps = maps.ToArray(),
                offset = offset,
                deaths = deaths,
                fieldsCount = fieldsCount,
                fieldsCountMapped = fieldsCountMapped,
                generation = generation,
                indexEndBody = indexEndBody,
                indexEndFields = indexEndFields,
                isReadingUntilCloseConnection = isReadingUntilCloseConnection,
                length = length,
                lengthAnalyzed = lengthAnalyzed,
                type = type,


                pool = default,
                debug = debug,
            };
        }

        public void BindToPool(IPoolReturn<HttpBinaryMapped> pool)
        {
            this.pool = pool!;
        }
    }
}