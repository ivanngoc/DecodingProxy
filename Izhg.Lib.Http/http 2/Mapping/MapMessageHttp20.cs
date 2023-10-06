using System;
using IziHardGames.Libs.HttpCommon.Http20;

namespace IziHardGames.Libs.ForHttp20.Maps
{
    public struct MapMessageHttp20
    {
        public FrameHttp20 frame;
        public ReadOnlyMemory<byte> payload;

        public MapMessageHttp20(FrameHttp20 frame, ReadOnlyMemory<byte> payload)
        {
            this.frame = frame;
            this.payload = payload;
        }

        public string ToStringInfo()
        {
            return $"{frame.ToStringInfo()} {ParseByte.ToHexStringFormated(payload)}";
        }
    }

    public struct MapHeadersHttp20
    {
        public ReadOnlyMemory<byte> segment;
    }
    public struct MapBodyHttp20
    {
        public ReadOnlyMemory<byte> segment;
    }
}