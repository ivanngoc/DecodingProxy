namespace IziHardGames.Libs.Cryptography.Tls12
{
    /// <summary>
    /// record layer frame's type
    /// <see cref="TlsEnums.ContentType"/>
    /// </summary>
    public enum ETlsProtocol : byte
    {
        /// <summary>
        /// https://www.rfc-editor.org/rfc/rfc5246#section-7.1
        /// </summary>
        ChangeCipherSpec = 0x14,
        /// <summary>
        /// https://www.rfc-editor.org/rfc/rfc5246#section-7.2
        /// </summary>
        AlertRecord = 0x15,
        /// <summary>
        /// https://www.rfc-editor.org/rfc/rfc5246#section-7.2
        /// </summary>
        Handshake = 0x16,
        /// <summary>
        /// https://www.rfc-editor.org/rfc/rfc5246#section-10
        /// </summary>
        ApplicationData = 0x17,
    }
}
