using System;
using IziHardGames.Libs.Binary.Readers;
using IziHardGames.Libs.Cryptography.Shared.Headers;
using IziHardGames.Libs.Cryptography;
using IziHardGames.Libs.Cryptography.Tls12;

namespace IziHardGames.NodeProxies.Nodes.Tls
{
    internal abstract class NodeTls : Node
    {
        internal static EGateProtocol DetectTls(DataFragment dataFragment)
        {
            var mem = dataFragment.ReadOnly;
            TlsRecord tlsRecord = BufferReader.ToStruct<TlsRecord>(mem);
            if (!tlsRecord.IsTls()) return EGateProtocol.TLS_ERROR;

            if (mem.Length >= ConstantsForTls.SIZE_RECORD_HANDSHAKE)
            {
                mem = mem.Slice(ConstantsForTls.SIZE_RECORD);
                while (mem.Length > 0)
                {
                    HandshakeHeader handshakeHeader = BufferReader.ToStructConsume<HandshakeHeader>(ref mem);
                    if (handshakeHeader.Type == ETlsTypeHandshakeMessage.ClientHello) return EGateProtocol.TLS_CLIENT;
                    if (handshakeHeader.Type == ETlsTypeHandshakeMessage.ServerHello) return EGateProtocol.TLS_SERVER;
                    mem = mem.Slice(handshakeHeader.Length);
                }
                return EGateProtocol.TLS_ERROR;
            }
            else
            {
                return EGateProtocol.TooShortToIdentify;
            }
        }
    }
}
