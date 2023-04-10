// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using HttpDecodingProxy.http;

namespace IziHardGames.Proxy
{
    public class Message
    {
        public EProxyType type;
        public List<string> raws = new List<string>();
        public Http11Fields http = new Http11Fields();

        public string DebugRaw => raws.Aggregate((x, y) => x + y);


        internal void AddLine(string line)
        {
            raws.Add(line);
        }

        public static implicit operator Http11Fields(Message m) => m.http;
    }
}