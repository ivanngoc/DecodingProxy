using System;
using System.Text;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Socks5.Headers;

namespace IziHardGames.NodeProxies.Nodes
{
    /// <summary>
    /// анализирует первый пакет и выявляет тип протокола
    /// </summary>
    internal class NodeGate : Node
    {
        internal static EGateProtocol DetectProtocol(DataFragment dataFragment)
        {
            var mem = dataFragment.buffer;
            if (mem.Length >= 5)
            {
                TlsRecord tlsRecord = BufferReader.ToStruct<TlsRecord>(mem);
                if (tlsRecord.IsTls()) return EGateProtocol.TLS;
                if (mem.Length > 6)
                {
                    string str = Encoding.UTF8.GetString(mem.Slice(0, 7).Span).ToUpper();
                    if (string.Equals(str, "CONNECT", StringComparison.InvariantCultureIgnoreCase)) return EGateProtocol.HTTP_CONNECT;
                    if (str.StartsWith("GET") || str.StartsWith("POST") || str.StartsWith("PUT")) return EGateProtocol.HTTP11;
                }
                ClientGreetingsSocks5 clientGreetingsSocks5 = BufferReader.ToStruct<ClientGreetingsSocks5>(mem);
                if (clientGreetingsSocks5.IsSocsk5())
                {
                    return EGateProtocol.SOCKS5;
                }
            }
            return EGateProtocol.Unknown;
        }
    }
}
