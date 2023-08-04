using IziHardGames.Libs.NonEngine.Memory;
using System.Threading.Tasks;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IHub<T>
    {
        public void Return(T connection);
        public void Use();
        public void Unuse();
        public void SetVersion(string version);
        public ValueTask<T> GetOrCreateAsync(string title, IPoolObjects<T> pool);
    }
}