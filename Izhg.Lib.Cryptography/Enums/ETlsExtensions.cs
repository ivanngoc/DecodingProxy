using IziHardGames.Libs.Cryptography;

namespace IziHardGames.Libs.Cryptography.Tls
{
    /// <summary>
    /// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml
    /// </summary>
    public enum ETlsExtensions : ushort
    {
        EXTENSION_SERVER_NAME = 0x00_00,                //0
        EXTENSION_STATUS_REQUEST = 0x00_05,             //5
        EXTENSION_SUPPORTED_GROUPS = 0x00_0a,           //10
        EXTENSION_EC_POINT_FORMATS = 0x00_0b,           //11
        EXTENSION_SIGNATURE_ALGORITHMS = 0x00_0d,       //13
        APPLICATION_LAYER_PROTOCOL_NEGOTIATION = 16,    //16
        EXTENSION_SCT = 0x00_12,                        //18
        EXTENDED_MASTER_SECRET = 23,                    //23
        RECORD_SIZE_LIMIT = 28,                         //28
        DELEGATED_CREDENTIAL = 34,                      //34
        SESSION_TICKET = 35,                            //35
        SUPPORTED_VERSIONS = 43,                        //43
        PSK_KEY_EXCHANGE_MODES = 45,                    //45
        KEY_SHARE = 51,                                 //51
        EXTENSION_H2 = 0x68_32,                         //26674
        //64251-65279 	Unassigned
        EXTENSION_RENEGOTIATION_INFO = 0xff_01,         //65281
    }

    public enum ETlsProtocolVersion : ushort
    {
        None,
        Tls10 = ConstantsForTls.CLIENT_VERSION_TLS10,
        Tls11 = ConstantsForTls.CLIENT_VERSION_TLS11,
        Tls12 = ConstantsForTls.CLIENT_VERSION_TLS12,
        Tls13 = ConstantsForTls.CLIENT_VERSION_TLS13,
    }
}