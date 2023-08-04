﻿using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using IziHardGames.Core;
using IziHardGames.Core.Buffers;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Buffers.Vectors;

namespace IziHardGames.Proxy.TcpDecoder
{
    public class TlcFrame
    {
        public static void FromStream(Stream stream)
        {

        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct TlcHelloFromClient
    {
        // ============================ Record Header
        /// <summary>
        /// <see cref="ConstantsTlc.HANDSHAKE_RECORD"/>
        /// </summary>
        [FieldOffset(0)] public byte type;                      //1
        /// <summary>
        /// <see cref="ConstantsTlc.CLIENT_VERSION10"/> | <see cref="ConstantsTlc.CLIENT_VERSION12"/>
        /// </summary>  
        [FieldOffset(1)] public short protocolVersion;          //2
        [FieldOffset(3)] public short msgSize;                  //2

        // ============================ HandshakeHeader
        [FieldOffset(5)] public byte handshakeMessageType;      //1
        [FieldOffset(6)] public Bytes3 sizeClientHello;         //3

        // ============================ Client Version
        /// <summary>
        /// <see cref="ConstantsTlc.CLIENT_VERSION10"/> | <see cref="ConstantsTlc.CLIENT_VERSION12"/>
        /// </summary>
        [FieldOffset(9)] public short clientVersion;            //2

        // ============================ Client Random
        [FieldOffset(11)] public Bytes32 clientRandom;          //32

        // ============================ Session ID
        /// <summary>
        /// <see cref="ConstantsTlc.SESSION_ID_NOT_PROVIDED"/>
        /// </summary>
        [FieldOffset(43)] public byte SessionID;                //1

        // ============================ Cipher Suites
        [FieldOffset(44)] public short lengthCipherSuiteData;

        public static void Read<T>(T frame) where T : IIndexReader<byte>, IReadOnlySpanProvider<byte>
        {
            byte type = frame[0];
            short protocolVersion = BufferReader.ToShort(frame[1], frame[2]);
            short msgSize = BufferReader.ToShort(frame[3], frame[4]);
            byte handshakeMessageType = frame[5];
            int sizeClientHello = BufferReader.ToInt32(frame[6], frame[7], frame[8]);
            short clientVersion = BufferReader.ToShort(frame[9], frame[10]);
            Bytes32 clientRandom = BufferReader.ToStruct<Bytes32>(frame.GetSpan(11, 32));
            byte sessionID = frame[43];
            short lengthCipherSuiteData = BufferReader.ToShort(frame[44], frame[45]);
            int i = 46;
            ReadOnlySpan<byte> cipherSuites = frame.GetSpan(46, lengthCipherSuiteData);
            i += lengthCipherSuiteData;
            byte lengthCompressionMethodsData = frame[i];
            i++;
            ReadOnlySpan<byte> compressionMethodsData = frame.GetSpan(i, lengthCompressionMethodsData);
            i += lengthCompressionMethodsData;
            short extensionsLength = BufferReader.ToShort(frame[i], frame[i + 1]);
            i += 2;

            int extensionsLengthLeft = extensionsLength;

#if DEBUG
            Console.WriteLine($"{nameof(type)} {ParseByte.ToHexStringFormated(type)}");
            Console.WriteLine($"{nameof(protocolVersion)} {ParseByte.ToHexStringFormated(protocolVersion)}");
            Console.WriteLine($"{nameof(msgSize)} {ParseByte.ToHexStringFormated(msgSize)}");
            Console.WriteLine($"{nameof(handshakeMessageType)} {ParseByte.ToHexStringFormated(handshakeMessageType)}");
            Console.WriteLine($"{nameof(sizeClientHello)} {ParseByte.ToHexStringFormated(sizeClientHello)}");
            Console.WriteLine($"{nameof(clientVersion)} {ParseByte.ToHexStringFormated(clientVersion)}");
            Console.WriteLine($"{nameof(clientRandom)} {ParseByte.ToHexStringFormated(clientRandom)}");
            Console.WriteLine($"{nameof(sessionID)} {ParseByte.ToHexStringFormated(sessionID)}");
            Console.WriteLine($"{nameof(lengthCipherSuiteData)} {ParseByte.ToHexStringFormated(lengthCipherSuiteData)}");
            Console.WriteLine($"{nameof(cipherSuites)} {ParseByte.ToHexStringFormated(cipherSuites)}");
            Console.WriteLine($"{nameof(lengthCompressionMethodsData)} {ParseByte.ToHexStringFormated(lengthCompressionMethodsData)}");
            Console.WriteLine($"{nameof(compressionMethodsData)} {ParseByte.ToHexStringFormated(compressionMethodsData)}");
            Console.WriteLine($"{nameof(extensionsLength)} {ParseByte.ToHexStringFormated(extensionsLength)}");
#endif

            while (extensionsLengthLeft > 0)
            {
                ETlcExtensions extensionType = (ETlcExtensions)BufferReader.ToUshort(frame[i], frame[i + 1]);
                i += 2;
                short extensionLength = BufferReader.ToShort(frame[i], frame[i + 1]);
                i += 2;

                int offsetData = 0;
                ReadOnlySpan<byte> extensionData = frame.GetSpan(i, extensionLength);
                i += extensionLength;
#if DEBUG
                Console.WriteLine($"Extension:{extensionType}. Length:{extensionLength}. Data:{ParseByte.ToHexStringFormated(extensionData)}");
#endif
                switch (extensionType)
                {
                    case ETlcExtensions.EXTENSION_SERVER_NAME:
                        {
                            ushort listEntrySize = BufferReader.ToUshort(extensionData);
                            offsetData += 2;
                            ReadOnlySpan<byte> listEntryData = extensionData.Slice(offsetData, listEntrySize);
                            byte listEntryType = extensionData[offsetData];
                            offsetData++;
                            ushort hostnameSize = BufferReader.ToUshort(extensionData[offsetData], extensionData[offsetData + 1]);
                            offsetData += 2;
                            ReadOnlySpan<byte> hostnameData = extensionData.Slice(offsetData, hostnameSize);
#if DEBUG
                            Console.WriteLine($"{nameof(hostnameData)} {hostnameData.ToStringUtf8()}");
#endif
                            break;
                        }
                    case ETlcExtensions.EXTENSION_STATUS_REQUEST:
                        {
                            break;
                        }
                    default: break;
                }
                extensionsLengthLeft -= extensionLength + 4;
            }
        }

        public static void TestZeroScans()
        {
            byte[] input = new byte[] { 0x16, 0x03, 0x01, 0x02, 0x01, 0x01, 0xFC, 0x03, 0x03, 0x4B, 0x34, 0x34, 0xE0, 0x77, 0x4B, 0x94, 0xE6, 0xCA, 0x59, 0x73, 0x04, 0xAB, 0x55, 0xBE, 0x5E, 0xB1, 0x98, 0x15, 0x67, 0xF4, 0x94, 0x5C, 0x8D, 0x61, 0x27, 0xA9, 0xC6, 0x8B, 0x73, 0x80, 0xC7, 0x20, 0xA5, 0xD9, 0x0D, 0xED, 0x64, 0xAF, 0x43, 0x29, 0xF3, 0x4C, 0xBF, 0xC3, 0x6E, 0xC3, 0x81, 0xDF, 0x90, 0x28, 0x3D, 0xA4, 0x98, 0x2C, 0x88, 0x15, 0x76, 0x9D, 0x46, 0x0C, 0x97, 0x0A, 0x8A, 0x51, 0x22, 0x13, 0x01, 0x13, 0x03, 0x13, 0x02, 0xC0, 0x2B, 0xC0, 0x2F, 0xCC, 0xA9, 0xCC, 0xA8, 0xC0, 0x2C, 0xC0, 0x30, 0xC0, 0x0A, 0xC0, 0x09, 0xC0, 0x13, 0xC0, 0x14, 0x9C, 0x9D, 0x2F, 0x35, 0x01, 0x01, 0x91, 0x12, 0x10, 0x0D, 0x7A, 0x65, 0x72, 0x6F, 0x73, 0x63, 0x61, 0x6E, 0x73, 0x2E, 0x63, 0x6F, 0x6D, 0x17, 0xFF, 0x01, 0x01, 0x0A, 0x0E, 0x0C, 0x1D, 0x17, 0x18, 0x19, 0x01, 0x01, 0x01, 0x0B, 0x02, 0x01, 0x23, 0x10, 0x0E, 0x0C, 0x02, 0x68, 0x32, 0x08, 0x68, 0x74, 0x74, 0x70, 0x2F, 0x31, 0x2E, 0x31, 0x05, 0x05, 0x01, 0x22, 0x0A, 0x08, 0x04, 0x03, 0x05, 0x03, 0x06, 0x03, 0x02, 0x03, 0x33, 0x6B, 0x69, 0x1D, 0x20, 0xF0, 0x42, 0xE7, 0xA6, 0x0C, 0xB6, 0x5F, 0xC2, 0xDC, 0x6B, 0x86, 0x66, 0xB4, 0xA2, 0xE2, 0x44, 0x5A, 0xB3, 0xB8, 0x36, 0xE4, 0x82, 0xAB, 0x20, 0xE9, 0x15, 0x7E, 0x91, 0x20, 0xE0, 0x16, 0x31, 0x17, 0x41, 0x04, 0xF3, 0x83, 0x46, 0x29, 0x68, 0xCF, 0xDA, 0x8D, 0xFB, 0x2E, 0xDA, 0x80, 0x01, 0x96, 0xC0, 0x3A, 0x61, 0xA6, 0x57, 0xCE, 0x80, 0xE2, 0x5A, 0x37, 0x2A, 0xC4, 0xC9, 0xBC, 0xA7, 0x14, 0xAE, 0x13, 0x28, 0x03, 0xFC, 0x8F, 0xDD, 0x55, 0xA9, 0xD5, 0x2A, 0xE2, 0x52, 0xCD, 0x4C, 0x9E, 0x5A, 0xFB, 0x1D, 0xEC, 0x5D, 0xD4, 0x59, 0x0E, 0xF8, 0x26, 0x01, 0x24, 0xC6, 0x8C, 0xA2, 0xD2, 0x54, 0xF4, 0x2B, 0x05, 0x04, 0x03, 0x04, 0x03, 0x03, 0x0D, 0x18, 0x16, 0x04, 0x03, 0x05, 0x03, 0x06, 0x03, 0x08, 0x04, 0x08, 0x05, 0x08, 0x06, 0x04, 0x01, 0x05, 0x01, 0x06, 0x01, 0x02, 0x03, 0x02, 0x01, 0x2D, 0x02, 0x01, 0x01, 0x1C, 0x02, 0x40, 0x01, 0x15, 0x89 };
            TlcHelloFromClient.Read<IndexReaderForArray<byte>>(input);
        }
        public static void Test12()
        {
            byte[] input12 = new byte[] { 0x16, 0x03, 0x01, 0x00, 0xa5, 0x01, 0x00, 0x00, 0xa1, 0x03, 0x03, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0x00, 0x00, 0x20, 0xcc, 0xa8, 0xcc, 0xa9, 0xc0, 0x2f, 0xc0, 0x30, 0xc0, 0x2b, 0xc0, 0x2c, 0xc0, 0x13, 0xc0, 0x09, 0xc0, 0x14, 0xc0, 0x0a, 0x00, 0x9c, 0x00, 0x9d, 0x00, 0x2f, 0x00, 0x35, 0xc0, 0x12, 0x00, 0x0a, 0x01, 0x00, 0x00, 0x58, 0x00, 0x00, 0x00, 0x18, 0x00, 0x16, 0x00, 0x00, 0x13, 0x65, 0x78, 0x61, 0x6d, 0x70, 0x6c, 0x65, 0x2e, 0x75, 0x6c, 0x66, 0x68, 0x65, 0x69, 0x6d, 0x2e, 0x6e, 0x65, 0x74, 0x00, 0x05, 0x00, 0x05, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0a, 0x00, 0x0a, 0x00, 0x08, 0x00, 0x1d, 0x00, 0x17, 0x00, 0x18, 0x00, 0x19, 0x00, 0x0b, 0x00, 0x02, 0x01, 0x00, 0x00, 0x0d, 0x00, 0x12, 0x00, 0x10, 0x04, 0x01, 0x04, 0x03, 0x05, 0x01, 0x05, 0x03, 0x06, 0x01, 0x06, 0x03, 0x02, 0x01, 0x02, 0x03, 0xff, 0x01, 0x00, 0x01, 0x00, 0x00, 0x12, 0x00, 0x00 };

            var frame = default(TlcHelloFromClient);
            TlcHelloFromClient.Read<IndexReaderForArray<byte>>(input12);
        }
    }

    public class ConstantsTlc
    {
        public const short CLIENT_VERSION10 = 0x0301;
        public const short CLIENT_VERSION11 = 0x0302;
        public const short CLIENT_VERSION12 = 0x0303;   //771

        public const byte HANDSHAKE_RECORD = 0x16;
        public const byte SESSION_ID_NOT_PROVIDED = 0x00;


    }

    public enum ETlcExtensions : ushort
    {
        EXTENSION_SERVER_NAME = 0x00_00,
        EXTENSION_STATUS_REQUEST = 0x00_05,
        EXTENSION_SUPPORTED_GROUPS = 0x00_0a,
        EXTENSION_EC_POINT_FORMATS = 0x00_0b,
        EXTENSION_SIGNATURE_ALGORITHMS = 0x00_0d,
        EXTENSION_SCT = 0x00_12,
        EXTENSION_RENEGOTIATION_INFO = 0xff_01,
    }
}