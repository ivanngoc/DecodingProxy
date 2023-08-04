using IziHardGames.Libs.Networking.Clients;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IConnection
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public void SetState(EConnectionState state);
    }
}