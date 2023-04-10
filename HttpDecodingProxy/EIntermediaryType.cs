namespace IziHardGames.Proxy.Http
{
    /// <summary>
    /// https://httpwg.org/specs/rfc9110.html#intermediaries
    /// </summary>
    public enum EIntermediaryType
    {
        None,
        Proxy,
        Gateway,
        Tunnel,
        /// <summary>
        /// Man In The Middle
        /// </summary>
        MITM,
    }
}