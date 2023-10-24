namespace IziHardGames.Libs.Cryptography.Readers
{
    /// <summary>
    /// Raw Reader
    /// </summary>
    public class TlsReader
    {
        protected readonly byte[] frameHello = new byte[1024];
        protected int length;
    }
}
