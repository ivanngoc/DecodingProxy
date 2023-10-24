namespace IziHardGames.Libs.Cryptography.Tls12
{
    /// <summary>
    /// <see cref="TlsEnums.ContentType"/>
    /// </summary>
    public enum ETlsTypeRecord : byte
    {
        ChangeCipherSpec = 0x14,
        AlertRecord = 0x15,
        Handshake = 0x16,
        ApplicationData = 0x17,
    }
}
