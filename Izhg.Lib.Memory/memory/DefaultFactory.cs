namespace IziHardGames.Pools.Abstractions.NetStd21
{
    public class DefaultFactory<T> : IPoolFactory<T> where T : new()
    {
        public T Create()
        {
            return new T();
        }
    }
}