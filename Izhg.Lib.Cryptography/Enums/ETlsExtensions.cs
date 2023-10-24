using IziHardGames.Libs.Cryptography;

namespace IziHardGames.Libs.Cryptography.Tls
{
    /// <summary>
    /// https://www.iana.org/assignments/tls-extensiontype-values/tls-extensiontype-values.xhtml
    /// https://www.rfc-editor.org/rfc/rfc5246#ref-TLSEXT
    /// </summary>
    public enum ETlsExtensions : ushort
    {
        EXTENSION_SERVER_NAME = 0x00_00,                //0
        max_fragment_length = 1,                //1
        client_certificate_url = 2,                //2  https://www.rfc-editor.org/rfc/rfc6066.html#section-1.1 https://www.rfc-editor.org/rfc/rfc6066.html#section-5
        //client_certificate_url = ,                //
        EXTENSION_STATUS_REQUEST = 0x00_05,             //5
        server_authz = 8,                //
        EXTENSION_SUPPORTED_GROUPS = 0x00_0a,           //10
        EXTENSION_EC_POINT_FORMATS = 0x00_0b,           //11
        EXTENSION_SIGNATURE_ALGORITHMS = 0x00_0d,       //13    https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.4
        use_srtp = 14,                //
        APPLICATION_LAYER_PROTOCOL_NEGOTIATION = 16,    //16
        EXTENSION_SCT = 0x00_12,                        //18
        UNKNOWN19 = 19,                    //19
        encrypt_then_mac = 22,                //
        EXTENDED_MASTER_SECRET = 23,                    //23
        token_binding = 24,                //
        RECORD_SIZE_LIMIT = 28,                         //28
        password_salt = 31,                //   https://www.rfc-editor.org/rfc/rfc8492.html#section-6
        ticket_pinning = 32,                //
        tls_cert_with_extern_psk = 33,                //
        DELEGATED_CREDENTIAL = 34,                      //34
        SESSION_TICKET = 35,                            //35
        SUPPORTED_VERSIONS = 43,                        //43
        PSK_KEY_EXCHANGE_MODES = 45,                    //45
        KEY_SHARE = 51,                                 //51
        UNKNOWN54 = 54,                                 //54    https://www.rfc-editor.org/rfc/rfc9146.html#name-new-entry-in-the-tls-extens
        EXTENSION_H2 = 0x68_32,                         //26674
        //64251-65279 	Unassigned
        EXTENSION_RENEGOTIATION_INFO = 0xff_01,         //65281
    }
}