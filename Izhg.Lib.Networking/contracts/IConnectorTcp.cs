using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IConnectorTcp
    {
        public Task ConnectAsyncTcp(string host, int port);
    }
}