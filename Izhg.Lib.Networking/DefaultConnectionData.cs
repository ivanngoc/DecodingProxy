using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.Libs.Networking.Contracts
{
    public class DefaultConnectionData : IConnectionData
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int Id { get; set; }
        public int Action { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }

        private IPoolReturn<DefaultConnectionData> pool;

        public void BindToPool(IPoolReturn<DefaultConnectionData> pool)
        {
            this.pool = pool;
        }

        public void Dispose()
        {
            pool.Return(this);
            pool = default;

            Host = string.Empty;
            Version = string.Empty;
            Status = string.Empty; 
            Port = 0;
            Id = 0;
        }
        public override string ToString()
        {
            return ToInfoConnectionData();
        }
        public string ToInfoConnectionData()
        {
            return $"ConnectionData. GetType():{GetType().FullName}; host:{Host}; port:{Port}; id:{Id}; action:{Action}; status:{Status}; version:{Version}";
        }
    }
}