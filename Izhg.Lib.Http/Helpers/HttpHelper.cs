using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.Text;

namespace IziHardGames.Libs.HttpCommon.Helpers
{
    public static class HttpHelper
    {
        public static Url DisassembleHost(in ReadOnlyMemory<byte> mem)
        {
            var slice = mem;

            return new Url(GetScheme(ref slice), GetHost(ref slice), GetPort(ref slice));

            (bool, ReadOnlyMemory<byte>) GetScheme(ref ReadOnlyMemory<byte> slice)
            {
                var copy = slice;
                if (Strings.IsStartWithCI(in copy, ConstantsForHttp.Urls.httpsArray))
                {
                    var result = slice.Slice(0, ConstantsForHttp.Urls.httpsArray.Length);
                    slice = slice.Slice(ConstantsForHttp.Urls.httpsArray.Length);
                    return (true, result);
                }
                else if (Strings.IsStartWithCI(in copy, ConstantsForHttp.Urls.httpArray))
                {
                    var result = slice.Slice(0, ConstantsForHttp.Urls.httpArray.Length);
                    slice = slice.Slice(ConstantsForHttp.Urls.httpArray.Length);
                    return (true, result);
                }
                return (false, default);
            }
            ReadOnlyMemory<byte> GetHost(ref ReadOnlyMemory<byte> slice)
            {
                int index = slice.IndexOf((byte)':');
                // can't be first at index [0]
                if (index > 0)
                {
                    var result = slice.Slice(0, index);
                    slice = slice.Slice(index + 1);
                    return result;
                }
                return default;
            }
            (bool, ReadOnlyMemory<byte>) GetPort(ref ReadOnlyMemory<byte> slice)
            {
                return (slice.IsContainDigit(), slice);
            }
        }
    }

    public readonly struct Url
    {
        public readonly ReadOnlyMemory<byte> scheme;
        public readonly ReadOnlyMemory<byte> host;
        public readonly ReadOnlyMemory<byte> port;
        public readonly bool isSchemePresented;
        public readonly bool isPortPresented;
        public int Port => port.ToInt32();
        public string Host => host.ToStringUtf8();
        public bool IsHttp => scheme.Length == ConstantsForHttp.Urls.httpArray.Length;
        public bool IsHttps => scheme.Length == ConstantsForHttp.Urls.httpsArray.Length;

        public Url((bool, ReadOnlyMemory<byte>) scheme, ReadOnlyMemory<byte> host, (bool, ReadOnlyMemory<byte>) port)
        {
            isSchemePresented = scheme.Item1;
            this.scheme = scheme.Item2;
            this.host = host;
            isPortPresented = port.Item1;
            this.port = port.Item2;
        }
    }
}
