// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Proxy.Sniffing.ForHttp;
using System;
using System.Net;
using System.Text;

namespace HttpDecodingProxy.ForHttp
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class ConstantsForHttp
    {
        public const int LENGTH_LF = 2;

        public const int MAX_INT_CHARS = 10;
        /// <summary>
        /// request-line = method SP request-target SP HTTP-version SP = Space 
        /// </summary>
        public const int TYPE_REQUEST = 1;
        /// <summary>
        /// status-line = HTTP-version SP status-code SP [ reason-phrase ]
        /// </summary>
        public const int TYPE_RESPONSE = 0;
        ///<see cref="HttpVersion"/>
        public const string version10 = "HTTP/1.0";
        public const string version11 = "HTTP/1.1";
        public const string version20 = "HTTP/2";
        public const string version30 = "HTTP/3";
        public static readonly ReadOnlyMemory<byte> memVersion10;
        public static readonly ReadOnlyMemory<byte> memVersion11;
        public static readonly ReadOnlyMemory<byte> memVersion20;
        public static readonly ReadOnlyMemory<byte> memVersion30;
        public static readonly byte[] bytesVersion10;
        public static readonly byte[] bytesVersion11;
        public static readonly byte[] bytesVersion20;
        public static readonly byte[] bytesVersion30;

        static ConstantsForHttp()
        {
            bytesVersion10 = UTF8Encoding.UTF8.GetBytes(version10);
            bytesVersion11 = UTF8Encoding.UTF8.GetBytes(version11);
            bytesVersion20 = UTF8Encoding.UTF8.GetBytes(version20);
            bytesVersion30 = UTF8Encoding.UTF8.GetBytes(version30);

            memVersion10 = new ReadOnlyMemory<byte>(bytesVersion10);
            memVersion11 = new ReadOnlyMemory<byte>(bytesVersion11);
            memVersion20 = new ReadOnlyMemory<byte>(bytesVersion20);
            memVersion30 = new ReadOnlyMemory<byte>(bytesVersion30);
        }

        /// <summary>
        /// https://httpwg.org/specs/rfc9110.html#status.codes
        /// </summary>
        public class StatusCodes
        {
            public const int INFORMATIONAL_100 = 100;


            public const int SUCCESSFUL_200 = 200;
            public const int NO_CONTENT_204 = 204;


            public const int REDIRECTION_300 = 300;
            public const int NOT_MODIFIED_304 = 304;


            public const int CLIENT_ERROR_400 = 400;


            public const int SERVER_ERROR_500 = 500;
        }

        public class FieldNames
        {
            public const string NAME_HOST = "Host";
            public const string NAME_CONNECTION = "Connection";
            public const string NAME_CONTENT_LENGTH = "Content-Length";
            public const string NAME_TRANSFER_ENCODING = "Transfer-Encoding";
            public const string NAME_KEEP_ALIVE = "Keep-Alive";
        }

        /// <summary>
        /// Field name till ':' inclusive
        /// </summary>
        public static class FieldSubstrings
        {
            public const string SUBSTRING_CONNECTION = $"{FieldNames.NAME_CONNECTION}: ";
            public const string SUBSTRING_CONTENT_LENGTH = "Content-Length: ";
            public const string SUBSTRING_CONTENT_TYPE = "Content-Type: ";
            public const string SUBSTRING_HOST = "Host: ";
            public const string SUBSTRING_PROXY_CONNECTION = "Proxy-Connection: ";
            public const string FIELD_TRANSFER_ENCODING = "Transfer-Encoding: ";
            public readonly static char[] charsHost;

            static FieldSubstrings()
            {
                charsHost = new char[] { 'H', 'O', 'S', 'T', ':', ' ' };
            }
        }

        public class FieldValues
        {
            public const string VALUE_CHUNKED = "chunked";
            public const string VALUE_CLOSE_CONNECTION = "Close";
        }

        /// <summary>
        /// Full length fields (field name + value) without line-endings
        /// </summary>
        public class Fields
        {
            public const string FIELD_CONNECTION_CLOSE = "Connection: close";
        }
        public class Responses
        {
            public const string SwitchProtocols = "HTTP/1.1 101 Connection Established\r\nConnection: upgrade\r\nUpgrade: HTTP/2.0\r\n\r\n";
            public const string OK11 = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
            //public const string OK11 = "HTTP/2 200 Connection Established\r\nConnection: upgrade\r\nUpgrade: HTTP/2.0\r\n\r\n";
            public const string OK2 = "HTTP/2 200 Connection Established\r\nConnection: close\r\n\r\n";

            public static byte[] bytesOk11 = Encoding.UTF8.GetBytes(OK11);
        }

        public static class Methods
        {
            public const string CONNECET = "CONNECT";
            public static char[] charsStartLineConnect = new char[] { 'C', 'O', 'N', 'N', 'E', 'C', 'T', ' ' };
            public static char[] charsStartLineGet = new char[] { 'G', 'E', 'T', ' ' };
            public static char[] charsStartLinePost = new char[] { 'P', 'O', 'S', 'T', ' ' };
            public static char[] charsStartLinePut = new char[] { 'P', 'U', 'T', ' ' };
        }

        public static class Events
        {
            public const int TYPE_ACTION_HANDLE_EVENT = 1;
        }

        public static class Urls
        {
            public readonly static ReadOnlyMemory<char> http;
            public readonly static ReadOnlyMemory<char> https;
            public readonly static char[] httpArray;
            public readonly static char[] httpsArray;

            static Urls()
            {
                httpArray = new char[] { 'h', 't', 't', 'p', ':', '/', '/' };
                httpsArray = new char[] { 'h', 't', 't', 'p', 's', ':', '/', '/' };
                http = new ReadOnlyMemory<char>(httpArray);
                https = new ReadOnlyMemory<char>(httpsArray);
            }
        }

        public static class StartLine
        {
            public readonly static byte[] CONNECT_AS_BYTES = new byte[] { 0x43, 0x4F, 0x4E, 0x4E, 0x45, 0x43, 0x54 };
        }
    }
}