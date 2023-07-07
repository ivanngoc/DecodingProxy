// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Libs.IO;
using IziHardGames.Libs.Networking.Pipelines;
using IziHardGames.Libs.NonEngine.Enumerators;
using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace HttpDecodingProxy.ForHttp
{
    public class HttpBinary : IHttpObject, IDisposable
    {
        public byte[]? buffer;
        private int length;
        public int offset;
        public int indexEndFields;
        /// <summary>
        /// Где кончается тело
        /// </summary>
        public int indexEndBody;
        private (int, int)[] maps;
        private int fieldsCount;
        private int fieldsCountTemp;
        private IPoolObjects<HttpBinary>? pool;
        public int type;
        public string debug;
        public bool isReadingUntilCloseConnection;
        private int generation;
        private int deaths;

        public int FieldsCount => fieldsCount;
        public int FieldsCountTemp => fieldsCountTemp;
        public int LengthBody => indexEndBody - indexEndFields;
        public int Length => length;
        /// <summary>
        /// Indluding enclosing \r\n
        /// </summary>
        public int LengthHeaders => indexEndFields - offset;
        public string Fields => ToStringFieldsFromMap();
        public string Raw => Encoding.UTF8.GetString(buffer.Take(length).ToArray());
        public bool IsCloseRequired => CheckCloseConnection();
        public Memory<byte> Memory => new Memory<byte>(buffer, offset, length);
        public Span<byte> Span => Memory.Span;
        public Memory<byte> this[string fieldName] => GetFieldValue(fieldName);
        public Span<byte> this[int index] => GetLineByIndex(index);

        public HttpBinary()
        {
            buffer = Array.Empty<byte>();
            maps = Array.Empty<(int, int)>();
        }
        public HttpBinary(int size)
        {
            buffer = ArrayPool<byte>.Shared.Rent(size);
            maps = Array.Empty<(int, int)>();
        }

        public HttpBinary Init()
        {
            generation++;
            return this;
        }
        public void Dispose()
        {
            deaths++;
            if (buffer.Length > 0) ArrayPool<byte>.Shared.Return(buffer);
            else
                buffer = Array.Empty<byte>();

            if (maps.Length > 0) ArrayPool<(int, int)>.Shared.Return(maps);
            else
                maps = Array.Empty<(int, int)>();

            pool.Return(this);
            pool = default;
            fieldsCount = default;
            fieldsCountTemp = default;
            length = default;
            offset = default;
            indexEndBody = default;
            indexEndFields = default;
            type = default;
            isReadingUntilCloseConnection = default;
        }

        public int EnsureCapacity(int toAdd)
        {
            int targetSize = toAdd + this.length;
            if (buffer.Length < targetSize)
            {
                var newarr = ArrayPool<byte>.Shared.Rent(targetSize);
                if (this.length > 0) Array.Copy(buffer, 0, newarr, 0, this.length);
                ArrayPool<byte>.Shared.Return(buffer);
                buffer = newarr;
            }
            return targetSize;
        }


        #region Getters
        private Memory<byte> GetFieldValue(string fieldName)
        {
            throw new NotImplementedException();
        }
        private Span<byte> GetLineByIndex(int index)
        {
            return new Span<byte>(buffer, maps[index].Item1, maps[index].Item2);
        }
        public Memory<byte> GetMemory()
        {
            return new Memory<byte>(buffer, offset, length);
        }
        /// <summary>
        /// <see cref="HttpBody.ReadBody(Stream, HttpObject)"/>
        /// </summary>
        /// <returns></returns>
        public int GetBodyLength()
        {
            if (CheckNoBody())
            {
                return 0;
            }
            if (CheckChunked())
            {
                return -2;
            }
            else if (TryGetFieldValueAsInt(HttpLibConstants.FieldNames.NAME_CONTENT_LENGTH, out int val))
            {
                return val;
            }
            return -3;
        }

        public (string, int) GetHostAndPortFromField()
        {
            if (TryGetFieldValueCI(HttpLibConstants.FieldNames.NAME_HOST, out var value))
            {
                int indexDelimter = value.IndexOf((byte)':');
                if (indexDelimter < 0) return (Encoding.UTF8.GetString(value), 80);
                var port = value.Slice(indexDelimter, value.Length - indexDelimter);
                Span<char> portChars = stackalloc char[port.Length];
                port.CopyAsChar(ref portChars);
                return (Encoding.UTF8.GetString(value.Slice(0, indexDelimter)), int.Parse(portChars));
            }
            throw new InvalidOperationException("This is field must be in request");
        }
        public EnumeratorArrayRef<(int, int)> GetEnumerator()
        {
            return new EnumeratorArrayRef<(int, int)>(maps, fieldsCount);
        }

        public bool TryGetFieldValueCI(string fieldName, out Span<byte> bytes)
        {
            for (int i = 0; i < fieldsCount; i++)
            {
                var span = new Span<byte>(buffer, maps[i].Item1, maps[i].Item2);
                if (span.Length < fieldName.Length) continue;
                for (int j = 0; j < fieldName.Length; j++)
                {
                    if (char.ToLowerInvariant((char)span[j]) != char.ToLowerInvariant((char)fieldName[j])) goto NEXT;
                }
                int offset = fieldName.Length + HttpLibConstants.LENGTH_LF;  // exclude substring: field-name:[BWS] .* \r\n
                int count = span.Length - (fieldName.Length + 4);
                if (count > 0)
                {
                    bytes = new Span<byte>(buffer, maps[i].Item1 + offset, count);
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
                var span = new Span<byte>(buffer, maps[i].Item1, maps[i].Item2);
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
            this.fieldsCountTemp++;
            int sizeToAdd = (int)readOnlySequence.Length;
            this.length = EnsureCapacity(sizeToAdd);
            Memory<byte> segment = new Memory<byte>(buffer, offset, sizeToAdd);
            readOnlySequence.CopyTo(segment.Span);
            return segment.Span;
        }

        public void AddBody(ReadOnlySequence<byte> readOnlySequence)
        {
            int lengthToCopy = (int)readOnlySequence.Length;
            readOnlySequence.CopyTo(new Span<byte>(buffer, this.length, lengthToCopy));
            this.length += lengthToCopy;
        }
        public void AddBodyForSure(ReadOnlySequence<byte> readOnlySequence)
        {
            var offset = this.length;
            this.length = EnsureCapacity((int)readOnlySequence.Length);
            int lengthToCopy = (int)readOnlySequence.Length;
            readOnlySequence.CopyTo(new Span<byte>(buffer, offset, lengthToCopy));
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
            var count = fieldsCountTemp;
            var newMap = ArrayPool<(int, int)>.Shared.Rent(count);
            if (maps.Length > 0)
            {
                Array.Copy(maps, 0, newMap, 0, count);
                ArrayPool<(int, int)>.Shared.Return(maps);
            }
            maps = newMap;
        }

        internal void MapField(int index, int offset, int length)
        {
            maps[index] = (offset, length);
        }

        public string ToStringFields()
        {
            return Encoding.UTF8.GetString(new Memory<byte>(buffer, offset, indexEndFields).Span);
        }
        public string ToStringFieldsFromMap()
        {
            return maps.Take(fieldsCount)
                  .Select(x => new Memory<byte>(buffer, x.Item1, x.Item2))
                  .Select(y => Encoding.UTF8.GetString(y.Span))
                  .Aggregate((x, y) => x + y);
        }

        /// <summary>
        /// <see cref="HttpFieldsV11.IsChunkedLast"/>
        /// </summary>
        /// <returns></returns>
        public bool CheckChunked()
        {
            if (TryGetFieldValueCI(HttpLibConstants.FieldNames.NAME_TRANSFER_ENCODING, out Span<byte> span))
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
            Span<byte> span = new Span<byte>(buffer, maps[0].Item1, maps[0].Item2);
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
        internal void BindToPool(IPoolObjects<HttpBinary> pool, int type)
        {
            this.pool = pool;
            this.type = type;
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
            fieldsCount = fieldsCountTemp;
            indexEndFields = length;
            debug = Encoding.UTF8.GetString(GetMemory().Span);
        }

        internal async Task<bool> TryReadBodyUntilEnd(TcpClientPiped client, CancellationTokenSource cts)
        {
            if (isReadingUntilCloseConnection)
            {
                await ReadBodyUntilEnd(client).ConfigureAwait(false);
                return true;
            }
            return false;
        }

        public async Task ReadBodyUntilEnd(TcpClientPiped client)
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
        public void ApplyControls(TcpClientPiped pipedTcpClient)
        {
            var version = GetVersion();

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

            if (TryGetFieldValueCI(HttpLibConstants.FieldNames.NAME_KEEP_ALIVE, out Span<byte> span))
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
                    Span<byte> slice = span.Slice(start, length);
                    int indexTimeout = slice.EndOfSubstringCI("timeout=");
                    if (indexTimeout > 0)
                    {
                        Span<byte> value = slice.Slice(indexTimeout + 1);
                        timeout = value.ParseToInt32();
                        isTimeout = true;
                    }
                    else
                    {
                        int indexMax = slice.EndOfSubstringCI("max=");
                        if (indexMax > 0)
                        {
                            Span<byte> value = slice.Slice(indexMax + 1);
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
            switch (GetVersion())
            {
                case EHttpVersion.None: throw new ArgumentOutOfRangeException();
                case EHttpVersion.Version10: return HttpLibConstants.version10;
                case EHttpVersion.Version11: return HttpLibConstants.version11;
                case EHttpVersion.Version20: return HttpLibConstants.version20;
                case EHttpVersion.Version30: return HttpLibConstants.version30;
                default: goto case EHttpVersion.None;
            }
        }
        public EHttpVersion GetVersion()
        {
            Span<byte> version;
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
    }
}