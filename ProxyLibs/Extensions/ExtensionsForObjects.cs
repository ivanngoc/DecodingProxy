// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using System.Runtime.Serialization.Formatters.Binary;

namespace ProxyLibs.Extensions
{

    public static class ExtensionsForObjects
    {
        public static T DeepCopy<T>(this T s) where T : class
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, s);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }
    }
}