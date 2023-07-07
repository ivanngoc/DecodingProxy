// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames;
using IziHardGames.Libs.Encodings;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy;
using IziHardGames.Proxy.Sniffing.ForHttp;
using ProxyLibs.Extensions;
using System.Buffers;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace HttpDecodingProxy.ForHttp
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9112.html#http.message
    /// </summary>
    public class HttpProxyMessage : IDisposable
    {
        public static readonly byte[] response200;
        public static readonly byte[] response404;
        public static readonly byte[] response500;

        public const byte CR = (byte)'\r';
        public const byte LF = (byte)'\n';

        public readonly HttpRequest request;
        public readonly HttpResponse response;
        public bool IsHttps { get; set; }
        private ManagerForHttpMessages manager;

        private readonly int id;
        private static int count;

        static HttpProxyMessage()
        {
            response200 = FormConnectResponse200();
            response404 = FormConnectResponse404();
            response500 = FormResponse500();
            count = 100;
        }

        public HttpProxyMessage()
        {
            request = new HttpRequest();
            request.Bind(this);

            response = new HttpResponse();
            response.Bind(this);

            count++;
            id = count;
        }

        public void Init(ManagerForHttpMessages manager)
        {
            this.manager = manager;
        }

        public static bool TryReadMsgInto(SslStream stream, HttpObject target)
        {
            ReadMsgInto(stream, target);
            return true;

            var buffer = ArrayPool<byte>.Shared.Rent(1);
            int readed = stream.Read(buffer, 0, 1);

            if (readed > 0)
            {
                return true;
            }
            return false;
        }
        public static void ReadMsgInto(Stream stream, HttpObject target)
        {
            HttpFieldsV11.ReadFields(stream, target);
            target.fields.ParseLines();
            target.fields.ApplyFields();
            target.body.ReadBody(stream, target);

            var version = target.fields.Version;
            if (version == HttpLibConstants.version20.ToLowerInvariant()) throw new NotSupportedException($"Protocol Version HTTP/2 not implemented yet");
            if (version != HttpLibConstants.version11.ToLowerInvariant()) throw new NotSupportedException($"Protocol Version other than HTTP/1.1 not implemented yet");
        }

        public void Dispose()
        {
            manager.Return(this);
            request.Dispose();
            response.Dispose();
            manager = default;
        }

        private static byte[] FormResponse500()
        {
            string resp = "HTTP/1.1 500 Internal Server Error\r\nConnection: close\r\n\r\n";
            return Encoding.UTF8.GetBytes(resp);
        }
        private static byte[] FormConnectResponse200()
        {
            //string resp = "HTTP/1.1 200 Connection Established\r\n\r\n";
            //string resp = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
            //string resp = "HTTP/1.1 200 OK";
            return Encoding.UTF8.GetBytes(HttpLibConstants.Responses.OK11);
        }
        private static byte[] FormConnectResponse404()
        {
            string resp =
@"HTTP/1.1 404 Not Found
Content-Type: text/html; charset=UTF-8
Content-Length: 1234

<!DOCTYPE html>
<html>
<head>
    <title>404 Not Found</title>
</head>
<body>
    <h1>Not Found</h1>
    <p>The requested URL /example was not found on this server.</p>
</body>
</html>";
            return Encoding.UTF8.GetBytes(resp);
        }

        public string ToStringInfo()
        {
            throw new NotImplementedException();
        }

        public StartOptions ToStartOptions()
        {
            return request.fields.ToStartOptions();
        }

    }
}