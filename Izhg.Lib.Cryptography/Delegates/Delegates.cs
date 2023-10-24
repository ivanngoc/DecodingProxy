using System;
using IziHardGames.Libs.Cryptography.Shared.Headers;

namespace IziHardGames.Libs.Cryptography.Delegates
{
    public delegate void TlsExtensionHandler(TlsExtension ext, in ReadOnlyMemory<byte> payload, ESide side);
    public struct Delegates
    {

    }
}
