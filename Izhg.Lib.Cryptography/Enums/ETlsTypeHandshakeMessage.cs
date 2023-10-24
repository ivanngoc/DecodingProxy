using IziHardGames.Libs.Cryptography.Attributes;

namespace IziHardGames.Libs.Cryptography.Tls12
{
    /// <summary>
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7
    /// https://www.rfc-editor.org/rfc/rfc5246#section-7.4
    /// </summary>
    public enum ETlsTypeHandshakeMessage : byte
    {
        [Side(Side = ESide.Server)] HelloRequest = 0x00,            //0 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.1
        [Side(Side = ESide.Client)] ClientHello = 0x01,             //1 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.2
        [Side(Side = ESide.Server)] ServerHello = 0x02,             //2 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.1.3
        [Side(Side = ESide.All)] Certificate = 0x0B,                //11 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.2 / https://www.rfc-editor.org/rfc/rfc5246#section-7.4.6
        [Side(Side = ESide.Server)] ServerKeyExchange = 0x0c,       //12 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.3
        [Side(Side = ESide.Server)] CertificateRequest = 0xd,       //13 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.4
        [Side(Side = ESide.Server)] ServerHelloDone = 0x0e,         //14 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.5
        [Side(Side = ESide.Client)] CertificateVerify = 0xf,        //15 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.8
        [Side(Side = ESide.Client)] ClientKeyExchange = 0x10,       //16 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.7
        [Side(Side = ESide.All)] Finished = 0x14,                   //20 https://www.rfc-editor.org/rfc/rfc5246#section-7.4.9
    }
}
