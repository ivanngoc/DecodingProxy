namespace IziHardGames.Libs.Cryptography.Tls
{
    public enum ETlsProtocolVersion : ushort
    {
        None,
        Tls10 = ConstantsForTls.CLIENT_VERSION_TLS10,
        Tls11 = ConstantsForTls.CLIENT_VERSION_TLS11,
        Tls12 = ConstantsForTls.CLIENT_VERSION_TLS12,
        Tls13 = ConstantsForTls.CLIENT_VERSION_TLS13,
    }
}