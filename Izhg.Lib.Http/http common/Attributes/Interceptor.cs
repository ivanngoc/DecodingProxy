// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System;

namespace IziHardGames.Libs.HttpCommon.Attributes
{
    /// <summary>
    /// Реализуется логика изменяющая данные 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class Interceptor : Attribute
    {

    }

    public class HttpHeaderAttribute : Attribute
    {

    }
    public class HttpBodyAttribute : Attribute
    {

    }
    public class HttpMessageAttribute : Attribute
    {

    }
}