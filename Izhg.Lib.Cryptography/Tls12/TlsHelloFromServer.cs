using System;
using IziHardGames.Core;
using IziHardGames.Core.Buffers;
using IziHardGames.Core.Buffers.Extensions;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Buffers.Vectors;
using IziHardGames.Proxy.TcpDecoder;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    public struct TlsHelloFromServer
    {
        public static void Read<T>(T frame) where T : IIndexReader<byte>, IReadOnlySpanProvider<byte>
        {
            //https://tls12.xargs.org/#server-hello
            //Record Header
            int i = default;
            byte type = frame[0];
            i++;
            ushort protocolVersion = frame.ToUshort(i);
            i += 2;
            ushort lengthHandshakeMessage = frame.ToUshort(i);
            i += 2; //5

            byte handshakeMessageType = frame[i];
            i++;    //6
            int lengthServerHello = BufferReader.ToInt32(frame[i], frame[i + 1], frame[i + 2]);
            i += 3; //9
            ushort serverVersion = frame.ToUshort(i);
            i += 2; //11
            Bytes32 serverRandom = BufferReader.ToStruct<Bytes32>(frame.GetSpan(i, 32));
            i += 32;//43
            byte sessionIDLength = frame[i];
            i++;    //44
            ReadOnlySpan<byte> sessionId = frame.GetSpan(i, sessionIDLength);
            i += sessionIDLength;
            ushort cipherSuite = frame.ToUshort(i);
            i += 2;
            byte compressionMethod = frame[i];
            i++;
            ushort extensionsLength = frame.ToUshort(i);
            i += 2;
#if DEBUG
            Console.WriteLine($"{nameof(type)} {ParseByte.ToHexStringFormated(type)}");
            Console.WriteLine($"{nameof(protocolVersion)} {ParseByte.ToHexStringFormated(protocolVersion)}");
            Console.WriteLine($"{nameof(lengthHandshakeMessage)} {ParseByte.ToHexStringFormated(lengthHandshakeMessage)}");
            Console.WriteLine($"{nameof(handshakeMessageType)} {ParseByte.ToHexStringFormated(handshakeMessageType)}");
            Console.WriteLine($"{nameof(lengthServerHello)} {ParseByte.ToHexStringFormated(lengthServerHello)}");
            Console.WriteLine($"{nameof(serverVersion)} {ParseByte.ToHexStringFormated(serverVersion)}");
            Console.WriteLine($"{nameof(serverRandom)} {ParseByte.ToHexStringFormated(serverRandom)}");
            Console.WriteLine($"{nameof(sessionIDLength)} {ParseByte.ToHexStringFormated(sessionIDLength)}");
            Console.WriteLine($"{nameof(sessionId)} {ParseByte.ToHexStringFormated(sessionId)}");
            Console.WriteLine($"{nameof(cipherSuite)} {ParseByte.ToHexStringFormated(cipherSuite)}");
            Console.WriteLine($"{nameof(compressionMethod)} {ParseByte.ToHexStringFormated(compressionMethod)}");
            Console.WriteLine($"{nameof(extensionsLength)} {ParseByte.ToHexStringFormated(extensionsLength)}");
#endif
            int extensionsLengthLeft = extensionsLength;
            while (extensionsLength > 0)
            {
                ETlsExtensions extensionType = (ETlsExtensions)BufferReader.ToUshort(frame[i], frame[i + 1]);
                i += 2;
                short extensionLength = BufferReader.ToShort(frame[i], frame[i + 1]);
                i += 2;

                int offsetData = 0;
                ReadOnlySpan<byte> extensionData = frame.GetSpan(i, extensionLength);
                i += extensionLength;
#if DEBUG
                Console.WriteLine($"Extension:{extensionType}. Length:{extensionLength}. Data:{ParseByte.ToHexStringFormated(extensionData)}");
#endif
                extensionsLengthLeft -= extensionLength + 4;
            }
        }
    }
}