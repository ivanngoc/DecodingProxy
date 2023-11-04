using System;
using IziHardGames.Libs.Cryptography.Shared.Headers;

namespace IziHardGames.Libs.Cryptography.Delegates
{
    public delegate void TlsHandlerForHandshakeExtensions(TlsExtension ext, in ReadOnlyMemory<byte> payload, ESide side, object container);
    public delegate void TlsHandlerForHandshake(in ReadOnlyMemory<byte> payload, ESide side, object container);
    public struct Delegates
    {

    }
}
