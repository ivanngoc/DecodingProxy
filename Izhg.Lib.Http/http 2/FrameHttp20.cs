using System;
using System.Buffers.Binary;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using IziHardGames.Libs.Buffers.Vectors;
using IziHardGames.Libs.ForHttp20;
using static IziHardGames.Libs.ForHttp20.ConstantsForHttp20;

namespace IziHardGames.Libs.HttpCommon.Http20
{
    [StructLayout(LayoutKind.Explicit, Size = ConstantsForHttp20.FRAME_DATA_SIZE_SETTINGS)]
    public struct FrameDataSettingHttp20
    {
        /// <summary>
        /// <see cref="ConstantsForHttp20.SettingsIdentifiers"/>
        /// </summary>
        [FieldOffset(0)] public ushort identifier;
        [FieldOffset(2)] public int value;
        public int ValueBE => value;
        public int ValueLE => BinaryPrimitives.ReverseEndianness(value);

        public string GetIdentifierVarName()
        {
            switch (identifier)
            {
                case ConstantsForHttp20.SettingsIdentifiers.SETTINGS_HEADER_TABLE_SIZE_BE: return "SETTINGS_HEADER_TABLE_SIZE";
                case ConstantsForHttp20.SettingsIdentifiers.SETTINGS_ENABLE_PUSH_BE: return "SETTINGS_ENABLE_PUSH";
                case ConstantsForHttp20.SettingsIdentifiers.SETTINGS_MAX_CONCURRENT_STREAMS_BE: return "SETTINGS_MAX_CONCURRENT_STREAMS";
                case ConstantsForHttp20.SettingsIdentifiers.SETTINGS_INITIAL_WINDOW_SIZE_BE: return "SETTINGS_INITIAL_WINDOW_SIZE";
                case ConstantsForHttp20.SettingsIdentifiers.SETTINGS_MAX_FRAME_SIZE_BE: return "SETTINGS_MAX_FRAME_SIZE";
                case ConstantsForHttp20.SettingsIdentifiers.SETTINGS_MAX_HEADER_LIST_SIZE_BE: return "SETTINGS_MAX_HEADER_LIST_SIZE";
                default: throw new ArgumentOutOfRangeException();
            }
        }

        internal bool IsHeaderTableSize()
        {
            return identifier == ConstantsForHttp20.SettingsIdentifiers.SETTINGS_HEADER_TABLE_SIZE_BE;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = ConstantsForHttp20.FRAME_SIZE)]
    public struct FrameHttp20
    {
        /// <summary>
        /// Payload length
        /// </summary>
        [FieldOffset(0)] public Bytes3 length;
        [FieldOffset(3)] public byte type;
        [FieldOffset(4)] public byte flags;
        [FieldOffset(5)] public uint streamIdentifier;
        public static FrameHttp20 Settings => new FrameHttp20() { type = 0x04 };

        public unsafe void WriteThisTo(Stream stream)
        {
            var copy = this;
            Span<FrameHttp20> span = new Span<FrameHttp20>(&copy, 1);
            Span<byte> bytes = MemoryMarshal.Cast<FrameHttp20, byte>(span);
            stream.Write(bytes);
        }
        public unsafe void WriteThisTo(NetworkStream stream)
        {
            var copy = this;
            Span<FrameHttp20> span = new Span<FrameHttp20>(&copy, 1);
            Span<byte> bytes = MemoryMarshal.Cast<FrameHttp20, byte>(span);
            stream.Write(bytes);
        }

        public string GetTypeName()
        {
            switch (type)
            {                
                case FrameTypes.DATA: return nameof(FrameTypes.DATA);
                case FrameTypes.HEADERS: return nameof(FrameTypes.HEADERS);
                case FrameTypes.PRIORITY: return nameof(FrameTypes.PRIORITY);
                case FrameTypes.RST_STREAM: return nameof(FrameTypes.RST_STREAM);
                case FrameTypes.SETTINGS: return nameof(FrameTypes.SETTINGS);
                case FrameTypes.PUSH_PROMISE: return nameof(FrameTypes.PUSH_PROMISE);
                case FrameTypes.PING: return nameof(FrameTypes.PING);
                case FrameTypes.GOAWAY: return nameof(FrameTypes.GOAWAY);
                case FrameTypes.WINDOW_UPDATE: return nameof(FrameTypes.WINDOW_UPDATE);
                case FrameTypes.CONTINUATION: return nameof(FrameTypes.CONTINUATION);
                default: throw new ArgumentOutOfRangeException();
            }
        }
        public string ToStringInfo()
        {
            throw new System.NotImplementedException();
        }
    }

    public enum EFrameType
    {
        None,
        Settings = 0x04,
    }

    public struct HeadersFrame
    {
        [StructLayout(LayoutKind.Explicit, Size = 1)]
        public struct Flags
        {
            [FieldOffset(0)] public byte value;
            public Flags(byte value) : this()
            {
                this.value = value;
            }
            public bool Priority => (value & 0b_0010_0000) != default;
            public bool Padded => (value & 0b_0000_1000) != default;
            public bool EndHeaders => (value & 0b_0000_0100) != default;
            public bool EndStream => (value & 0b_0000_0001) != default;

            public static implicit operator byte(Flags d) => d.value;
            public static implicit operator Flags(byte b) => new Flags(b);

            public static bool IsPrioritySet(byte value) => (value & 0b_0010_0000) != default;
            public static bool IsPaddedSet(byte value) => (value & 0b_0000_1000) != default;
            public static bool IsEndHeadersSet(byte value) => (value & 0b_0000_0100) != default;
            public static bool IsEndStreamSet(byte value) => (value & 0b_0000_0001) != default;
        }
    }
}
