// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace IziHardGames.Proxy
{
    public class DecodingProxyServerConfig
    {
        public static DecodingProxyServerConfig Singleton { get; set; }
        public List<string> HostsForSniffing { get; set; } = new List<string>();        
    }
}