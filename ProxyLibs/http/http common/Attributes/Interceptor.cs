// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace HttpDecodingProxy.ForHttp
{
    /// <summary>
    /// Реализуется логика изменяющая данные 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class Interceptor : Attribute
    {

    }
}