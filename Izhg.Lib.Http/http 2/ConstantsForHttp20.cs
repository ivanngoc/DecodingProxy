namespace IziHardGames.Libs.ForHttp20
{
    public class ConstantsForHttp20
    {
        public const int FRAME_SIZE = 9;
        public const int FRAME_DATA_SIZE_SETTINGS = 6;
        public const int CLIENT_PREFACE_SIZE = 24;
        public const string CLIENT_PREFACE = "PRI * HTTP/2.0\r\n\r\nSM\r\n\r\n";
        public static readonly byte[] clientPrefaceBytes = new byte[] {
        0x50,0x52,0x49,0x20,
        0x2a,0x20,0x48,0x54,
        0x54,0x50,0x2f,0x32,
        0x2e,0x30,0x0d,0x0a,
        0x0d,0x0a,0x53,0x4d,
        0x0d,0x0a,0x0d,0x0a
        };

        public class SettingsIdentifiers
        {
            public const ushort SETTINGS_HEADER_TABLE_SIZE_LE = 0x00_01;
            public const ushort SETTINGS_ENABLE_PUSH_LE = 0x02;
            public const ushort SETTINGS_MAX_CONCURRENT_STREAMS_LE = 0x00_03;
            public const ushort SETTINGS_INITIAL_WINDOW_SIZE_LE = 0x00_04;
            public const ushort SETTINGS_MAX_FRAME_SIZE_LE = 0x00_05;
            public const ushort SETTINGS_MAX_HEADER_LIST_SIZE_LE = 0x00_06;


            public const ushort SETTINGS_HEADER_TABLE_SIZE_BE = 0x01_00;
            public const ushort SETTINGS_ENABLE_PUSH_BE = 0x02_00;
            public const ushort SETTINGS_MAX_CONCURRENT_STREAMS_BE = 0x03_00;
            public const ushort SETTINGS_INITIAL_WINDOW_SIZE_BE = 0x04_00;
            public const ushort SETTINGS_MAX_FRAME_SIZE_BE = 0x05_00;
            public const ushort SETTINGS_MAX_HEADER_LIST_SIZE_BE = 0x06_00;
        }

        public class FrameTypes
        {
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#DATA
            /// </summary>
            public const byte DATA = 0x00;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#HEADERS
            /// </summary>
            public const byte HEADERS = 0x01;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#PRIORITY
            /// </summary>
            public const byte PRIORITY = 0x02;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#RST_STREAM
            /// </summary>
            public const byte RST_STREAM = 0x03;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#SettingFormat
            /// </summary>
            public const byte SETTINGS = 0x04;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#PUSH_PROMISE
            /// </summary>
            public const byte PUSH_PROMISE = 0x05;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#PING
            /// </summary>
            public const byte PING = 0x06;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#GOAWAY
            /// </summary>
            public const byte GOAWAY = 0x07;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#GOAWAY
            /// </summary>
            public const byte WINDOW_UPDATE = 0x08;
            /// <summary>
            /// https://httpwg.org/specs/rfc9113.html#CONTINUATION
            /// </summary>
            public const byte CONTINUATION = 0x09;
        }

        public class HPACK
        {
            public const int LENGTH_STATIC_TABLE = 62;

            public const byte MASK_INDEX = 0b_1000_0000;
            /// <summary>
            /// https://httpwg.org/specs/rfc7541.html#indexed.header.representation
            /// </summary>
            public const byte MASK_INDEX_REVERSE = 0b_0111_1111;
            /// <summary>
            /// https://httpwg.org/specs/rfc7541.html#literal.header.with.incremental.indexing
            /// </summary>
            public const byte PATTERN_INDEXED_HEADER_FIELD_INCREMENTAL = 0b_0100_0000;
            public const byte MASK_INDEXED_HEADER_FIELD_INCREMENTAL = 0b_0011_1111;
            /// <summary>
            /// https://httpwg.org/specs/rfc7541.html#literal.header.never.indexed
            /// </summary>
            public const byte PATTERN_INDEXED_HEADER_FIELD_NEVER_INDEX = 0b_0001_0000;
            /// <summary>
            /// https://httpwg.org/specs/rfc7541.html#literal.header.without.indexing
            /// </summary>
            public const byte PATTERN_INDEXED_HEADER_FIELD_NO_INDEX = 0b_1111_0000;

        }
    }
}
