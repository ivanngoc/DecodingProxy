// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System;
using System.Net;

namespace IziHardGames.Networking.Helpers
{
    public static class HelperIpAddress
    {
        public static IPAddress FindIpByHostNameAndDNS(string hostName, string v)
        {
            var adresses = Dns.GetHostAddresses(hostName);
            if (adresses.Length > 0) return adresses[0];
            throw new NotImplementedException();
        }
    }
}