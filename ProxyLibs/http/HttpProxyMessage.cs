// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames;
using IziHardGames.Libs.Encodings;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy;
using IziHardGames.Proxy.Sniffing.Http;
using ProxyLibs.Extensions;
using System.Buffers;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace HttpDecodingProxy.http
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9112.html#http.message
    /// </summary>
    public class HttpProxyMessage : IDisposable
    {
        public static readonly byte[] response200;
        public static readonly byte[] response500;

        public const byte CR = (byte)'\r';
        public const byte LF = (byte)'\n';

        public readonly HttpRequest request;
        public readonly HttpResponse response;
        public bool IsHttps { get; set; }
        public ManagerForHttpMessages manager;
        public HttpAgentConnection agent;
        private readonly int id;
        private static int count;

        static HttpProxyMessage()
        {
            response500 = FormResponse500();
            response200 = FormConnectResponse200();
            count = 100;
        }

        public HttpProxyMessage()
        {
            request = new HttpRequest(this);
            response = new HttpResponse(this);
            count++;
            id = count;
        }

        public void Bind(HttpAgentConnection connection)
        {
            this.agent = connection;
        }

        public void ReadMsgInto(Stream stream, NetworkStream ns, HttpOject target)
        {
            target.fields.ReadFields(stream, ns, target);
            target.fields.ParseLines();
            target.body.ReadBody(stream, target);
        }

        private void Decode(Span<byte> input)
        {
            BrotliDecoder brotliDecoder = new BrotliDecoder();
            //brotliDecoder.Decompress(input, new Span<byte>());
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            manager.Return(this);
            request.Dispose();
            response.Dispose();
            manager = default;
            agent = default;
        }

        private static byte[] FormResponse500()
        {
            string resp = "HTTP/1.1 500 Internal Server Error\r\nConnection: close\r\n\r\n";
            return Encoding.UTF8.GetBytes(resp);
        }
        private static byte[] FormConnectResponse200()
        {
            string resp = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
            //string resp = "HTTP/1.1 200 OK";
            return Encoding.UTF8.GetBytes(resp);
        }

        public string ToStringInfo()
        {
            throw new NotImplementedException();
        }
    }
}