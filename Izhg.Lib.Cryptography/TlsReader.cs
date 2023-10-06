namespace IziHardGames.Libs.Cryptography
{
    public class TlsReader
    {
        protected readonly byte[] frameHello = new byte[1024];
        protected int length;
    }
}
