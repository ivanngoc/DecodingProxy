// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames;
using IziHardGames.Libs.Encodings;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpDecodingProxy.http
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
    public class Http11Fields : IDisposable
    {
        public const string version11 = "HTTP/1.1";
        public const string KEEP_ALIVE_VALUE = "Keep-Alive";
        public const string KEEP_ALIVE_FIELD = "Keep-Alive:";
        public const string CONTENT_TYPE_HTML = "text/html";

        public bool IsRequest { get; set; }
        public bool IsResponse { get; set; }

        public string Version { get; set; }
        public int StatusCode { get; set; }
        public string StatusText { get; set; }
        public string Method { get; set; }
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

        private int tempIndexLineStart;
        public readonly List<string> fields = new List<string>(16);

        /*
        CONNECT mozilla.cloudflare-dns.com:443 HTTP/1.1
        User-Agent: Mozilla/5.0 (rv:111.0) Gecko/20100101 /111.0.1
        Proxy-Connection: keep-alive
        Connection: keep-alive
        Host: mozilla.cloudflare-dns.com:443
        */

        public void ReadFields(Stream stream, NetworkStream ns, HttpOject obj)
        {
            Http11Fields fields = obj.fields;
            bool isEndSequence = false;
            var sb = obj.sb;

            LineBegin(obj);

            while (true)
            {
                if (ns.DataAvailable)
                {
                    int i = stream.ReadByte();

                    if (i == -1)
                    {
                        obj.IsErrorReadMsg = true;
                        break;
                    }
                    char c = (char)i;
                    sb.Append(c);

                    if (c == '\r')
                    {
                        char next = (char)stream.ReadByte();
                        sb.Append(next);

                        if (next == '\n')
                        {
                            LineRestart(obj);
                            // /r/n/r/n  - indicated that fields part is end
                            if (isEndSequence)
                            {
                                return;
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
        }

        private void LineBegin(HttpOject obj)
        {
            tempIndexLineStart = obj.sb.Length;
        }
        private void LineRestart(HttpOject obj)
        {
            var sb = obj.sb;
            var temp = tempIndexLineStart;
            LineBegin(obj);
            obj.fields.fields.Add(sb.ToString(temp, sb.Length - temp - 2));
        }
        /// <summary>
        /// https://httpwg.org/specs/rfc9112.html#persistent.tear-down
        /// </summary>
        /// <param name="tearDown"></param>
        /// <returns></returns>
        public bool TryFindTearDown(out string tearDown)
        {
            foreach (var line in fields)
            {
                if (line.Contains("Connection: close"))
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
            foreach (var line in fields)
            {
                if (line.Contains("Content-Length:"))
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

        public Http11Fields()
        {
            Dispose();
        }

        public void Dispose()
        {
            fields.Clear();

            Version = string.Empty;
            Method = string.Empty;
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

            IsRequest = default;
            IsResponse = default;

            TransferEncoding = string.Empty;
            TransferEncodingValues = Array.Empty<string>();
        }

        public void ParseLines()
        {
            InitLine = fields[0];
            Method = InitLine.Substring(0, InitLine.IndexOf(' '));

            if (IsResponse)
            {
                var split = InitLine.Split(' ');
                Version = split[0];
                StatusCode = int.Parse(split[1]);
                StatusText = split[2];

                if (IsStatusCodeRange(Http.StatusCodes.SUCCESSFUL_200))
                {
                    Logger.LogLine($"Status code", ConsoleColor.Red);
                }
            }

            if (IsRequest)
            {
                if (InitLine.Contains("HTTP/"))
                {
                    Version = InitLine.Substring(InitLine.IndexOf("HTTP/"));
                }
            }

            if (InitLine.Contains(WebRequestMethods.Http.Connect))
            {
                IsConnectRequired = true;
            }

            for (int i = 1; i < fields.Count; i++)
            {
                var field = fields[i];

                if (field.Contains("Host:"))
                {
                    Host = field;
                    HostValue = field.Substring(6);
                    var split = HostValue.Split(':');

                    if (split.Length == 2)
                    {
                        HostAddress = split[0];
                        HostPort = int.Parse(split[1]);
                    }
                    else
                    {
                        HostAddress = HostValue;
                        HostPort = 80;
                    }
                }
                else if (field.Contains("Connection:"))
                {
                    Connection = field;
                    ConnectionControl = field.Substring(11 + 1);
                }
                else if (field.Contains("Proxy-Connection:"))
                {
                    ProxyConnection = field;
                    ProxyConnectionControl = field.Substring(17 + 1);
                }
                else if (field.Contains(KEEP_ALIVE_FIELD))
                {
                    KeepAlive = field;
                }
                else if (field.Contains("Content-Type:"))
                {
                    ContentType = field;
                }
                else if (field.Contains("Transfer-Encoding:"))
                {
                    TransferEncoding = field;
                    TransferEncodingValues = field.Substring(19).Split(" ");
                    IsGzip = TransferEncodingValues.Contains("gzip");
                    IsChunked = TransferEncodingValues.Contains("chunked");
                }
                else
                {
                    Logger.LogException(new NotImplementedException($"Field Parser Not Implemented: {field}"));
                }
            }
        }

        public string FindValue(string fieldName)
        {
            foreach (var field in fields)
            {
                if (field.Contains("fieldName"))
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
            return TransferEncodingValues.Last() == "chunked";
        }

        public string ToStringInfo()
        {
            string result = string.Empty;

            foreach (var field in fields)
            {
                result += field + Environment.NewLine;
            }
            return result;
        }
    }
}