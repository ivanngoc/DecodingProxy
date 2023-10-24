// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames;
using IziHardGames.Libs.NonEngine.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using static HttpDecodingProxy.ForHttp.ConstantsForHttp;

namespace HttpDecodingProxy.ForHttp
{
  
    /// <summary>
    /// https://httpwg.org/specs/rfc9110.html#field.connection<br/>
    /// </summary>
    public enum EConnection
    {
        None,
        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#compatibility.with.http.1.0.persistent.connections<br/>
        /// Connection: keep-alive
        /// </summary>
        KeepAlive,
        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#persistent.tear-down<br/>
        /// Connection: close
        /// </summary>
        Close,
    }

    /// <summary>
    /// https://httpwg.org/specs/rfc9110.html#rfc.section.7.6.1
    /// </summary>
    [Serializable]
    public class HttpFieldsV11 : IDisposable
    {

        public const string KEEP_ALIVE_VALUE = "Keep-Alive";
        public const string KEEP_ALIVE_FIELD = "Keep-Alive:";
        public const string CONTENT_TYPE_HTML = "text/html";

        public bool IsRequest { get; set; }
        public bool IsResponse { get; set; }

        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        public string Method { get; set; }
        public string RelativeReference { get; set; }

        /// <summary>
        /// mozilla.cloudflare-dns.com:443
        /// </summary>
        public string ConnectAddress { get; set; }
        /// <summary>
        ///  HTTP/1.1
        /// </summary>
        public string ConnectProtocol { get; set; }
        /// <summary>
        /// Mozilla/5.0 (rv:111.0) Gecko/20100101 /111.0.1
        /// </summary>
        public string UserAgent { get; set; }
        /// <summary>
        /// keep-alive
        /// </summary>
        public string ProxyConnection { get; set; }
        /// <summary>
        /// Host: mozilla.cloudflare-dns.com:443
        /// </summary>
        public string Connection { get; set; }
        /// <summary>
        /// Host: mozilla.cloudflare-dns.com:443
        /// </summary>
        public string Host { get; set; }
        public string HostAddress { get; set; }

        /// <summary>
        /// Reset after: "Host:"
        /// </summary>
        public string HostValue { get; set; }

        public int HostPort { get; set; }

        /// <summary>
        /// Proxy-Authorization: basic aGVsbG86d29ybGQ=
        /// </summary>
        public string ProxyAuthorization { get; set; }
        public string InitLine { get; set; }


        /// <summary>
        /// value of [Proxy-Connection: keep-alive]
        /// </summary>
        public string ProxyConnectionControl { get; set; }
        /// <summary>
        /// value of [Connection: keep-alive]
        /// https://httpwg.org/specs/rfc9112.html#persistent.tear-down
        /// https://httpwg.org/specs/rfc9112.html#rfc.section.C.2.2
        /// https://httpwg.org/specs/rfc9110.html#field.upgrade
        /// </summary>
        public string ConnectionControl { get; set; }
        /// <summary>
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Keep-Alive
        /// </summary>
        public string KeepAlive { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ContentType { get; set; }
        public string ContentTypeCharset { get; set; }
        public string ContentTypeValue { get; set; }

        public string ContentBody { get; set; }
        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#body.content-length<br/>
        /// </summary>
        public int ContentLength { get; set; }
        public string ContentLengthField { get; set; }

        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#field.transfer-encoding<br/>
        /// Transfer-Encoding: gzip, chunked
        /// </summary>
        public string TransferEncoding { get; set; }
        public string[] TransferEncodingValues { get; set; } = Array.Empty<string>();

        public bool IsGzip { get; set; }
        public bool IsChunked { get; set; }

        /// <summary>
        /// https://httpwg.org/specs/rfc9110.html#field.content-encoding<br/>
        /// Content-Encoding: gzip 
        /// </summary>
        public string ContentEncoding { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool IsConnectRequired { get; set; }
        public string Info => ToStringInfo();

        public readonly List<string> lines = new List<string>(16);
        private readonly Dictionary<string, Action<string>> handlerPerField;
        public readonly Dictionary<string, string> valuePerFieldName;
        private ILogger logger;
        public static ILogger loggerShared;

        public HttpFieldsV11()
        {
            logger = loggerShared;

            handlerPerField = new Dictionary<string, Action<string>>()
            {
                [ConstantsForHttp.FieldNames.NAME_CONNECTION] = null,
            };
            valuePerFieldName = new Dictionary<string, string>(16);
        }

        /*
        CONNECT mozilla.cloudflare-dns.com:443 HTTP/1.1
        User-Agent: Mozilla/5.0 (rv:111.0) Gecko/20100101 /111.0.1
        Proxy-Connection: keep-alive
        Connection: keep-alive
        Host: mozilla.cloudflare-dns.com:443
        */
        // NOTE:DO NOT ACCESS RAW STREAM PROPERTIES. IT MIGHT PROCEED REEDING INTO DIFFERENT BUFFER
        public static void ReadFields(Stream stream, HttpObject obj)
        {
            HttpFieldsV11 fields = obj.fields;
            bool isEndSequence = false;
            var sb = obj.sb;
            int tempIndexLineStart = default;
            // ssl stream can allocate single sized array
            var buffer = ArrayPool<byte>.Shared.Rent(1);

            LineBegin(obj, out tempIndexLineStart);

            while (true)
            {
                int readed = stream.Read(buffer, 0, 1);

                if (readed > 0)
                {
                    int i = buffer[0];

                    if (i == -1)
                    {
                        obj.isErrorReadMsg = true;
                        continue;
                    }
                    char c = (char)i;
                    sb.Append(c);

                    if (c == '\r')
                    {
                        char next = (char)stream.ReadByte();
                        sb.Append(next);

                        if (next == '\n')
                        {
                            LineRestart(obj, ref tempIndexLineStart);
                            // /r/n/r/n  - indicated that fields part is end
                            if (isEndSequence)
                            {
                                break;
                            }
                            isEndSequence = true;
                        }
                    }
                    else
                    {
                        isEndSequence = false;
                    }
                }
            }
            ArrayPool<byte>.Shared.Return(buffer);
        }

        private static void LineBegin(HttpObject obj, out int tempIndexLineStart)
        {
            tempIndexLineStart = obj.sb.Length;
        }
        private static void LineRestart(HttpObject obj, ref int tempIndexLineStart)
        {
            var sb = obj.sb;
            var temp = tempIndexLineStart;
            LineBegin(obj, out tempIndexLineStart);
            obj.fields.lines.Add(sb.ToString(temp, sb.Length - temp - 2).ToLowerInvariant());
        }
        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#persistent.tear-down
        /// </summary>
        /// <param name="tearDown"></param>
        /// <returns></returns>
        public bool TryFindTearDown(out string tearDown)
        {
            foreach (var line in lines)
            {
                if (line.Contains(ConstantsForHttp.Fields.FIELD_CONNECTION_CLOSE, StringComparison.InvariantCultureIgnoreCase))
                {
                    tearDown = line;
                    return true;
                }
            }
            tearDown = string.Empty;
            return false;
        }
        public bool TryFindContentLengthLine(out string contentLength)
        {
            foreach (var line in lines)
            {
                if (line.Contains(ConstantsForHttp.FieldSubstrings.SUBSTRING_CONTENT_LENGTH, StringComparison.InvariantCultureIgnoreCase))
                {
                    contentLength = line;
                    return true;
                }
            }
            contentLength = string.Empty;
            return false;
        }

        public bool TryFindTransferEncoding(out string[] encodings)
        {
            if (!string.IsNullOrEmpty(TransferEncoding))
            {
                encodings = TransferEncodingValues;
                return true;
            }
            encodings = Array.Empty<string>();
            return false;
        }



        public void Dispose()
        {
            lines.Clear();
            valuePerFieldName.Clear();

            Version = string.Empty;
            Method = string.Empty;
            RelativeReference = string.Empty;
            StatusCode = -1;
            StatusText = string.Empty;

            ConnectAddress = string.Empty;
            ConnectProtocol = string.Empty;
            UserAgent = string.Empty;
            ProxyConnection = string.Empty;
            Connection = string.Empty;
            Host = string.Empty;
            HostAddress = string.Empty;
            HostValue = string.Empty;
            HostPort = default;
            ProxyAuthorization = string.Empty;
            InitLine = string.Empty;
            ProxyConnectionControl = string.Empty;
            ConnectionControl = string.Empty;
            KeepAlive = string.Empty;

            ContentBody = string.Empty;
            ContentLength = default;
            ContentLengthField = string.Empty;

            ContentType = string.Empty;
            ContentTypeCharset = string.Empty;
            ContentTypeValue = string.Empty;

            ContentEncoding = string.Empty;

            IsGzip = default;
            IsChunked = default;


            TransferEncoding = string.Empty;
            TransferEncodingValues = Array.Empty<string>();
        }

        public void ParseLines()
        {
            InitLine = lines[0];

            if (IsResponse)
            {
                var split = InitLine.Split(' ');
                Version = split[0];
                StatusCode = int.Parse(split[1]);
                StatusText = split[2];

                if (IsStatusCodeRange(ConstantsForHttp.StatusCodes.SUCCESSFUL_200))
                {
                    MyLogger.LogLine($"Status code", ConsoleColor.Red);
                }
            }
            else if (IsRequest)
            {
                var split = InitLine.Split(' ');
                Method = split[0];
                RelativeReference = split[1];
                Version = split[2];
            }

            if (InitLine.Contains(WebRequestMethods.Http.Connect, StringComparison.InvariantCultureIgnoreCase))
            {
                IsConnectRequired = true;
            }
            //exclude reading last field (\r\n\r\n) that is enclosure
            int count = lines.Count - 1;

            for (int i = 1; i < count; i++)
            {
                var field = lines[i];

                var split = field.Split(':');

                if (valuePerFieldName.TryGetValue(split[0], out var value))
                {   // для дублирующихся полей будет браться последнее значение
                    valuePerFieldName[split[0]] = split[1];
                }
                else
                {
                    valuePerFieldName.Add(split[0], split[1]);
                }

                if (field.Contains(FieldSubstrings.SUBSTRING_HOST, StringComparison.InvariantCultureIgnoreCase))
                {
                    Host = field;
                    HostValue = field.Substring(6);
                    var split2 = HostValue.Split(":");

                    if (split2.Length == 2)
                    {
                        HostAddress = split2[0];
                        HostPort = int.Parse(split2[1]);
                    }
                    else
                    {
                        HostAddress = HostValue;
                        HostPort = 80;
                    }
                }
                else if (field.Contains(FieldSubstrings.SUBSTRING_CONNECTION, StringComparison.InvariantCultureIgnoreCase))
                {
                    Connection = field;
                    ConnectionControl = field.Substring(11 + 1);
                }
                else if (field.Contains(FieldSubstrings.SUBSTRING_PROXY_CONNECTION, StringComparison.InvariantCultureIgnoreCase))
                {
                    ProxyConnection = field;
                    ProxyConnectionControl = field.Substring(17 + 1);
                }
                else if (field.Contains(KEEP_ALIVE_FIELD, StringComparison.InvariantCultureIgnoreCase))
                {
                    KeepAlive = field;
                }
                else if (field.Contains(FieldSubstrings.SUBSTRING_CONTENT_TYPE, StringComparison.InvariantCultureIgnoreCase))
                {
                    ContentType = field;
                }
                else if (field.Contains(FieldSubstrings.FIELD_TRANSFER_ENCODING, StringComparison.InvariantCultureIgnoreCase))
                {
                    TransferEncoding = field.ToLowerInvariant();
                    TransferEncodingValues = field.Substring(19).Split(" ").Select(x => x.ToLowerInvariant()).ToArray();
                    IsGzip = TransferEncodingValues.Contains("gzip");
                    IsChunked = TransferEncodingValues.Contains("chunked");
                }
                //else
                //{
                //    Logger.LogLine($"Field Parser Not Implemented: {field}");
                //}
            }
        }

        public void ApplyFields()
        {
            foreach (var pair in handlerPerField)
            {
                if (pair.Value is null)
                {
                    logger.LogWarning($"Реактивный обработчик для http поля [{pair.Key}] не установлен"); continue;
                }
                if (valuePerFieldName.TryGetValue(pair.Key, out var value))
                {
                    pair.Value(value);
                }
            }
        }

        public string FindValue(string fieldName)
        {
            foreach (var field in lines)
            {
                if (field.Contains("fieldName", StringComparison.InvariantCultureIgnoreCase))
                {
                    return field.Substring(fieldName.Length);
                }
            }
            throw new ArgumentOutOfRangeException(fieldName);
        }

        public bool TryGetKeepAliveValues(out int timeout, out int max)
        {
            if (KeepAlive.Length > 0)
            {
                ReadTimeout(KeepAlive, out timeout, out max);
                return timeout >= 0 && max >= 0;
            }
            timeout = -3;
            max = -3;
            return false;
        }

        public static void ReadTimeout(string input, out int timeout, out int max)
        {
            int i = 0;

            for (i = 0; i < KEEP_ALIVE_FIELD.Length; i++)
            {
                if (input[i] != KEEP_ALIVE_FIELD[i])
                {
                    timeout = -1;
                    max = -1;
                    return;
                }
            }

            if (i != ' ')
            {
                timeout = -2;
                max = -2;
                return;
            }
            i++;

            Span<char> span = stackalloc char[10];
            int spanPos = default;

            for (i = input.IndexOf("timeout=") + 8; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsDigit(c))
                {
                    span[spanPos] = c;
                    spanPos++;
                }
                else
                {
                    break;
                }
            }
            timeout = int.Parse(span);
            spanPos = default;

            for (i = input.IndexOf("max=") + 4; i < input.Length; i++)
            {
                char c = input[i];
                if (char.IsDigit(c))
                {
                    span[spanPos] = c;
                    spanPos++;
                }
                else
                {
                    break;
                }
            }
            max = int.Parse(span);
        }

        public bool IsMethod(string head)
        {
            return Method == head;
        }

        internal bool IsStatusCode(int code)
        {
            return this.StatusCode == code;
        }
        internal bool IsStatusCodeRange(int range)
        {
            return (this.StatusCode / 100) == (range / 100);
        }

        internal bool IsChunkedLast()
        {
            bool res = TransferEncodingValues.Last().Contains(ConstantsForHttp.FieldValues.VALUE_CHUNKED.ToLowerInvariant());
#if DEBUG
            if (TransferEncodingValues.Contains("chunked") && !res) throw new NotImplementedException("по RFC chunked должен быть последним");
#endif
            return res;
        }

        public string ToStringInfo()
        {
            string result = string.Empty;

            foreach (var field in lines)
            {
                result += field + Environment.NewLine;
            }
            return result;
        }

        public bool CheckCloseConnection()
        {
            foreach (var field in lines)
            {
                if (field.Contains("connection") && field.Contains("close")) return true;
            }
            return false;
        }

        public StartOptions ToStartOptions()
        {
            var pool = PoolObjectsConcurent<StartOptions>.Shared;
            var options = pool.Rent();
            options.Init(pool);

            options.HostAndPort = this.HostValue;
            options.Host = this.Host;
            options.HostAddress = this.HostAddress;
            options.HostPort = this.HostPort;
            options.IsHttps = this.IsConnectRequired;
            options.IsConnectRequired = this.IsConnectRequired;
            return options;
        }
        /// <summary>
        /// <see cref="HttpBody.ReadBody(Stream, HttpObject)"/>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int AnalyzBodyLength()
        {
            if (TryFindTransferEncoding(out string[] encodings))
            {
                if (IsChunkedLast())
                {
                    return -2;
                }
            }
            if (TryFindContentLengthLine(out string contentLength))
            {
                int size = int.Parse(contentLength.Substring(16));
                return size;
            }
            return -1;
        }

        public ReadOnlySpan<char> AddField(ReadOnlySequence<byte> readOnlySequence)
        {
            var line = Encoding.UTF8.GetString(readOnlySequence).ToLowerInvariant();
            lines.Add(line);
            return line.AsSpan();
        }
        public void AddField(Span<byte> line)
        {
            lines.Add(Encoding.UTF8.GetString(line).ToLowerInvariant());
        }
    }
}