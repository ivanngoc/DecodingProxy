// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace HttpDecodingProxy.http
{

    public class Http
    {
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


            public const int CLIENT_ERROR_400  = 400;


            public const int SERVER_ERROR_500  = 500;
        }
    }
}