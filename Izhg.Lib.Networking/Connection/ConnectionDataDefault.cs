using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Libs.Networking.States;

namespace IziHardGames.Libs.Networking.SocketLevel
{
    public class ConnectionDataDefault : IConnectionData
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1;
        public int Id { get; set; }
        public int Action { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public string connectionKey = string.Empty; 

        public ENegotioanions negotioanions = ENegotioanions.None;

        public void SetData(string host, int port)
        {
            Host = host;
            Port = port;
            connectionKey = $"{host}:{port}";
        }

        public virtual void Dispose()
        {
            Host = string.Empty;
            Version = string.Empty;
            Status = string.Empty;
            Port = -1;
            Id = 0;
            connectionKey = string.Empty;
            negotioanions = ENegotioanions.None;
        }
        public virtual string ToInfoConnectionData()
        {
            return $"ConnectionData. GetType():{GetType().FullName}; host:{Host}; port:{Port}; id:{Id}; action:{Action}; status:{Status}; version:{Version}";
        }
        public override string ToString()
        {
            return ToInfoConnectionData();
        }
    }
}