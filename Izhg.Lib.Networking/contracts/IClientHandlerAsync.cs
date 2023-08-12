using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IClientHandlerAsync<T>
    {
        Task<T> HandleClientAsync(T client, CancellationToken token = default);
    }
}
