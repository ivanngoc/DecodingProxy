namespace IziHardGames.Libs.Cryptography.Shared.Headers
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc6066.html#section-5
    /// </summary>
    public enum ECertChainType : byte
    {
        individual_certs = 0,
        pkipath = 1,
    }
}
