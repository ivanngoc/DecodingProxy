using System;

namespace IziHardGames.Libs.ForHttp11.Maps
{
    public struct MapMessageHttp11
    {
        public MapHeadersHttp1 headers;
        public MapBodyHttp1 body;
    }

    public struct MapHeadersHttp1
    {
        public ReadOnlyMemory<byte> segment;
    }
    public struct MapBodyHttp1
    {
        public ReadOnlyMemory<byte> segment;
    }
}