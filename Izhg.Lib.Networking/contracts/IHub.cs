using System.Threading.Tasks;
using IziHardGames.Libs.Networking.States;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.Contracts
{
    /// <summary>
    /// Концентратор подключений
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHub<T>
    {
        public Task Return(T connection);
        public void Use();
        public void Unuse();
        public void SetVersion(string version);
        public ValueTask<T> GetOrCreateAsync(EConnectionFlags flags, string title, IPoolObjects<T> pool);
    }
}