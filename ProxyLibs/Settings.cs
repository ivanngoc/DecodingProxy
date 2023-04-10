// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy
{

    public class GlobalProxySettings
    {
        public  ProxySettings proxySettings = new ProxySettings();
        public X509Certificate2 cert;
        public string certCN;
    }
}