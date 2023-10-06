using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IziHardGames.Lib.Text;
using IziHardGames.Libs.NonEngine.Enumerables;
using IziHardGames.Libs.NonEngine.Enumerators;
using IziHardGames.Libs.Text;
using static HttpDecodingProxy.ForHttp.ConstantsForHttp;

namespace HttpDecodingProxy.ForHttp
{
    public static class ReaderHttpBlind
    {
        public static ReadOnlyMemory<byte> FindHeadersHttp11(in ReadOnlyMemory<byte> mem)
        {
            var span = mem.Span;
            int index = mem.IndexAfterRnRn();
            if (index > 0) return mem.Slice(0, index);
            return default;
        }
        public static async Task<int> AwaitHeadersWithEmptyBody(byte[] rawReadBuffer, Socket socket)
        {
            int size = default;
            while (true)
            {
                int readed = await socket.ReceiveAsync(rawReadBuffer, SocketFlags.None).ConfigureAwait(false);
                size += readed;
                if (size > 4)
                {
                    if (rawReadBuffer[size - 4] == '\r' && rawReadBuffer[size - 3] == '\n' && rawReadBuffer[size - 2] == '\r' && rawReadBuffer[size - 1] == '\n')
                    {
                        return size;
                    }
                }
            }
            throw new System.NotImplementedException();
        } 
        
        public static async Task<int> AwaitHeadersWithEmptyBody(byte[] rawReadBuffer, Stream stream)
        {
            int size = default;
            while (true)
            {
                int readed = await stream.ReadAsync(rawReadBuffer).ConfigureAwait(false);
                size += readed;
                if (size > 4)
                {
                    if (rawReadBuffer[size - 4] == '\r' && rawReadBuffer[size - 3] == '\n' && rawReadBuffer[size - 2] == '\r' && rawReadBuffer[size - 1] == '\n')
                    {
                        return size;
                    }
                }
            }
            throw new System.NotImplementedException();
        }
        public static (string, int) FindHostAndPort(ReadOnlyMemory<byte> memWithMsg, int offset = default)
        {
            var spanLine = ReaderForBufferAsTextUtf16.ReadLine(memWithMsg);
            EnumerableForSpanWhitespace enumerator = new EnumerableForSpanWhitespace(spanLine);

            foreach (var line in enumerator)
            {

            }
            throw new System.NotImplementedException();
        }

        public static bool TryReadStartLine(byte[] buffer, int start, int offset, out StartLineReadResult result)
        {
            return TryReadStartLine(new ReadOnlyMemory<byte>(buffer, start, offset), out result);
        }
        public static bool TryReadStartLine(in ReadOnlyMemory<byte> buffer, out StartLineReadResult result)
        {
#if DEBUG
            string s = Encoding.UTF8.GetString(buffer.Span);
#endif
            result = default;
            EStartLine flags = EStartLine.None;
            // ReadMethod
            if (ReaderForBufferAsTextUtf16.TryReadLine(buffer, out var line))
            {
                EnumeratorForSpanWhitespace enumerator = new EnumeratorForSpanWhitespace(buffer);
                if (enumerator.MoveNext())
                {
                    var methodSlice = enumerator.Current;

                    if (methodSlice.CompareWith("CONNECT"))
                    {
                        flags |= EStartLine.MethodConnect;
                    }
                    else if (methodSlice.CompareWith("GET"))
                    {
                        flags |= EStartLine.MethodGet;
                    }
                    else if (methodSlice.CompareWith("POST"))
                    {
                        flags |= EStartLine.MethodPost;
                    }
                    else
                    {
                        flags |= EStartLine.MethodNotImplemented;
                    }
                    result.flags = flags;
                    result.NextStep();

                    if (enumerator.MoveNext())
                    {
                        var memHost = enumerator.Current;
                        var connectionLine = memHost;
                        var span = connectionLine.Span;
                        var offset = 0;

                        if (Strings.IsStartWithCI(in span, Urls.httpArray))
                        {
                            connectionLine = connectionLine.Slice(Urls.httpArray.Length);
                            offset = Urls.httpArray.Length;
                            result.flags |= EStartLine.HttpPresented;
                        }
                        else if (Strings.IsStartWithCI(in span, Urls.httpsArray))
                        {
                            connectionLine = connectionLine.Slice(Urls.httpsArray.Length);
                            offset = Urls.httpsArray.Length;
                            result.flags |= EStartLine.HttpsPresented;
                        }

                        int indexColon = connectionLine.IndexOf(':');
                        if (indexColon > 0)
                        {
                            result.host = memHost.Slice(0, indexColon + offset);
                            int indexStartPort = indexColon + 1;
                            var splitPort = connectionLine.Slice(indexStartPort, connectionLine.Length - indexStartPort);
                            Span<char> charsPort = stackalloc char[splitPort.Length];
                            var portSpan = splitPort.Span;
                            for (int i = 0; i < charsPort.Length; i++)
                            {
                                charsPort[i] = (char)portSpan[i];
                            }
                            result.port = int.Parse(charsPort);
                            result.flags |= EStartLine.PortPresented;
                        }
                        else
                        {
                            result.host = connectionLine;
                        }

                        result.NextStep();

                        if (enumerator.MoveNext())
                        {
                            var splitVersion = enumerator.Current;
                            if (splitVersion.CompareWith(ConstantsForHttp.memVersion11))
                            {
                                result.httpVersion = EHttpVersion.Version11;
                                return true;
                            }
                            else
                            {
                                throw new NotSupportedException(Encoding.UTF8.GetString(splitVersion.Span));
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private static object locker = new object();
        public static bool TryFindHostFeild(in ReadOnlyMemory<byte> memWithMsg, out ReadOnlyMemory<byte> result)
        {
            lock (locker)
            {
                EnumeratorForSpanNewLine enumerator = new EnumeratorForSpanNewLine(in memWithMsg);
                var span = memWithMsg.Span;
                while (enumerator.MoveNext())
                {
                    var mem = enumerator.Current;
                    if (Strings.IsStartWithCI(in mem, ConstantsForHttp.FieldSubstrings.charsHost))
                    {
                        result = mem.Slice(ConstantsForHttp.FieldSubstrings.charsHost.Length);
                        return true;
                    }
                }
            }
            result = default;
            return false;
        }
        public static ReadOnlyMemory<byte> FindHostFeild(in ReadOnlyMemory<byte> memWithMsg)
        {
            EnumeratorForSpanNewLine enumerator = new EnumeratorForSpanNewLine(in memWithMsg);
            var span = memWithMsg.Span;
            while (enumerator.MoveNext())
            {
                var mem = enumerator.Current;
                if (Strings.IsStartWithCI(in span, ConstantsForHttp.FieldSubstrings.charsHost))
                {
                    return mem.Slice(ConstantsForHttp.FieldSubstrings.charsHost.Length);
                }
            }
            return default;
        }


    }

    public struct StartLineReadResult
    {
        public int port;
        public int step;
        public EStartLine flags;
        public EHttpVersion httpVersion;
        public ReadOnlyMemory<byte> host;
        public string Host => Encoding.UTF8.GetString(host.Span);

        public StartLineReadResult(EStartLine flags, ReadOnlyMemory<byte> host, int port, EHttpVersion httpVersion) : this()
        {
            this.flags = flags;
            this.host = host;
            this.port = port;
            this.httpVersion = httpVersion;
        }
        internal void NextStep()
        {
            step++;
        }

        public string ToStringFlags()
        {
            string result = string.Empty;
            var values = Enum.GetValues(typeof(EStartLine));

            foreach (var value in values)
            {
                if (flags.HasFlag((EStartLine)value)) result += $"{value.ToString()}|";
            }
            return result;
        }
        public string ToStringInfo()
        {
            return $"Flags:{ToStringFlags()}; host:{host}; port:{port}; version:{httpVersion}";
        }
    }

    [Flags]
    public enum EStartLine
    {
        Error = -1,
        None = 0,
        MethodConnect = 1 << 0,
        MethodGet = 1 << 1,
        MethodPost = 1 << 2,
        MethodPut = 1 << 3,
        Request = 1 << 4,
        Response = 1 << 5,
        MethodNotImplemented = 1 << 6,
        HttpPresented = 1 << 7,
        HttpsPresented = 1 << 8,
        PortPresented = 1 << 9,
    }
}