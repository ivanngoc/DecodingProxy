// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.IO;

namespace IziHardGames.Proxy
{
    public static class Settings
    {
        public static bool IsLogToFile { get; set; } = false;
    }

    public class GlobalProxySettings
    {
        public ProxySettings proxySettings = new ProxySettings();
        public X509Certificate2 cert;
        public string certCN;

        // Firefox
        public const int DEAULT_MAX_PERSISTENT_CONNECTIONS_PER_PROXY = 32;
        public const int DEAULT_HTTP_MAX_CONNECTIONS = 900;
    }

    public class ProxyRule
    {
        [JsonPropertyName("protocol")] public string protocol { get; set; }
        [JsonPropertyName("mode")] public string mode { get; set; }
        [JsonPropertyName("domain")] public string domain { get; set; }
        [JsonPropertyName("options")] public int[] options { get; set; }
    }

    public class ConfigJson
    {
        public static string configJson;
        public static string PathCertForged => (string)root["path_cert_cache_forged"];
        public static string PathCertOriginal => (string)root["path_cert_cache_original"];

        public static JsonArray Rules { get; set; }
        private static JsonObject root;


        [JsonPropertyName("port_http")] public int portHttp { get; set; }
        [JsonPropertyName("port_https")] public int portHttps { get; set; }
        [JsonPropertyName("path_cert_cache_forged")] public string path_cert_cache_forged { get; set; }
        [JsonPropertyName("path_cert_cache_original")] public string path_cert_cache_original { get; set; }
        [JsonPropertyName("rules")] public ProxyRule[] rules { get; set; }
        public static string path_ca_cert => (string)root["path_ca_cert"];
        public static string path_ca_key => (string)root["path_ca_key"];

        public static void Init()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "config.json");
            string json = File.ReadAllText(path);
            ConfigJson.configJson = json;

            JsonObject jObj = root = JsonNode.Parse(json).AsObject();
            Rules = jObj["rules"].AsArray();
        }

        public static void EnsureConfigExist()
        {
            string fn = "config.json";

            if (!File.Exists(fn))
            {
                Logger.LogLine("Config not founded. Begin creating...");

                var fs = File.Create(fn);
                fs.Write(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new ConfigJson())));
                fs.Dispose();
                Logger.LogLine("Config were created!");
            }
        }
    }
}