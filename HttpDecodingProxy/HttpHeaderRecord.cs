using System;

namespace IziHardGames.Libs.ForHttp.Common
{
    public struct HttpMessageRecord
    {
        public HttpHeaderRecord headers;
        public long datetime;
    }
    public struct HttpHeaderRecord
    {
        public ReadOnlyMemory<byte> Name;
        public ReadOnlyMemory<byte> Value;
    }
}
