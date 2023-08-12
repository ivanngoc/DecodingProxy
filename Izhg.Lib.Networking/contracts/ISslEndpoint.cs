using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface ISslEndpoint
    {
        Task AuthAsClientAsync();
    }
}