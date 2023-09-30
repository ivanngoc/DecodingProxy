using System.Threading.Tasks;

namespace IziHardGames.Libs.Async
{
    public struct AsyncOperation<T>
    {
        public int id;
        public T operation;
    }
}
