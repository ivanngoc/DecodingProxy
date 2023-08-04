// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Proxy.Sniffing.ForHttp;
using System.Net;
using System.Text;

namespace HttpDecodingProxy.ForHttp
{
    /// <summary>
    /// Constants
    /// </summary>
    public class HttpLibConstants
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
        public class FieldSubstrings
        {
            public const string SUBSTRING_CONNECTION = $"{FieldNames.NAME_CONNECTION}: ";
            public const string SUBSTRING_CONTENT_LENGTH = "Content-Length: ";
            public const string SUBSTRING_CONTENT_TYPE = "Content-Type: ";
            public const string SUBSTRING_HOST = "Host: ";
            public const string SUBSTRING_PROXY_CONNECTION = "Proxy-Connection: ";
            public const string FIELD_TRANSFER_ENCODING = "Transfer-Encoding: ";
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
            //public const string OK11 = "HTTP/1.1 200 Connection Established\r\nConnection: close\r\n\r\n";
            public const string OK11 = "HTTP/2 200 Connection Established\r\nConnection: upgrade\r\nUpgrade: HTTP/2.0\r\n\r\n";
            public const string OK2 = "HTTP/2 200 Connection Established\r\nConnection: close\r\n\r\n";

            public static byte[] bytesOk11 = Encoding.UTF8.GetBytes(OK11);
        }
    }
}