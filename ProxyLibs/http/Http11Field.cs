// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace HttpDecodingProxy.http
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9110.html#field.lines<br/>
    /// In case of combined values 
    /// </summary>
    public class Http11Field : IDisposable
    {
        public int Count { get; set; }

        public void Dispose()
        {
            Count = default;
        }
    }
}