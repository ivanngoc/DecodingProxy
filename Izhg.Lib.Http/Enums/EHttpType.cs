// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Net;

namespace HttpDecodingProxy.ForHttp
{
    public enum EHttpMethod
    {
        None,
        GET,
        POST,
        PUT,
        CONNECT,
        NOT_IMPLEMENTED,
    }

    /// <summary>
    /// <see cref="HttpVersion"/>
    /// </summary>
    public enum EHttpVersion
    {
        None,
        Version10,
        Version11,
        Version20,
        Version30,
    }

    public enum EHttpType
    {
        None,
        Field,
        FieldsDelimeter,
        FieldBodyDelimeter,
        Body,
        Enclosure,
    }
}