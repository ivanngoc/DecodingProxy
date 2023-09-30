using System;

namespace IziHardGames.Libs.ForHttp.Http20
{
    public class StreamHttp20
    {
        private EStreamHttp20State state;
        /// <summary>
        /// <see cref="FrameHttp20.streamIdentifier"/>
        /// </summary>
        private int streamIdentifier;

        public static StreamHttp20 OpenStream()
        {
            return new StreamHttp20();
        }
    }

    /// <summary>
    /// https://httpwg.org/specs/rfc9113.html#StreamStates
    /// </summary>
    public enum EStreamHttp20State
    {
        None,
        Idle,
        HalfClosed,
        Reserved,
        Open,
    }
}
