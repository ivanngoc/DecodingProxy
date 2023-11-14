namespace IziHardGames.Socks5.Enums
{
    public enum EAuth : byte
    {
        NoAuthRequired = 0x00,
        GSSAPI = 0x01,
        UserPassword = 0x02,

        //IANA
        Challenge = 0x03,
        Unassigned = 0x04,
        ChallengeResponseAuthenticationMethod = 0x05,
        SecureSocketsLayer = 0x06,
        NDSAuthentication = 0x07,
        MultiAuthenticationFramework = 0x08,
        JSONParameterBlock = 0x09,
    }

    public enum EAdrType : byte
    {
        None = 0,
        /// <summary>
        /// 4 bytes
        /// </summary>
        IPv4 = 0x01,
        /// <summary>
        /// 1 byte length, followed 1-255 bytes domain name
        /// </summary>
        DomainName = 0x03,
        /// <summary>
        /// 16 bytes
        /// </summary>
        IPv6 = 0x04,
    }

    public enum EReply : byte
    {
        RequestGranted = 0x00,
        GeneralFailure = 0x01,
        ConnectionNotAllowedByRuleset = 0x02,
        NetworkUnreachable = 0x03,
        HostUnreachable = 0x04,
        ConnectionRefusedByDestinationHost = 0x05,
        TTLExpired = 0x06,
        CommandNotSupportedProtocolError = 0x07,
        AddressTypeNotSupported = 0x08,
    }

    /// <summary>
    /// https://datatracker.ietf.org/doc/html/rfc1928#section-4
    /// </summary>
    public enum ECmd : byte
    {
        CONNECT = 0x01,
        BIND = 0x02,
        UDP_ASSOCIATE = 0x03,
    }

    public enum ESocksType : byte
    {
        SOCKS4 = 0x04,
        SOCKS5 = 0x05,
    }
}
