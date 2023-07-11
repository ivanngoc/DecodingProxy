// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Libs.NonEngine.Memory;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public class SslClientPiped : TcpClientPiped
    {
        private X509Certificate2 certificate;
        public void Init(X509Certificate2 cert, X509Certificate2 ca)
        {
            SslStream sslStream = new SslStream(this);
        }
    }
}