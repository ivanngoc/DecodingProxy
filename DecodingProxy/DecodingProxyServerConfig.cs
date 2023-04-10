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

        public static void EnsureConfigExist()
        {
            if (Singleton == null)
            {
                Singleton = new DecodingProxyServerConfig();
            }
            string fn = "config.json";

            if (!File.Exists(fn))
            {
                var fs = File.Create(fn);
                fs.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Singleton)));
                fs.Dispose();
            }
        }
    }
}