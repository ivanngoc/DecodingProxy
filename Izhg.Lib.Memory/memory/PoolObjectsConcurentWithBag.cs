using System.Collections.Concurrent;

namespace IziHardGames.Pools.Abstractions.NetStd21
{
    public class PoolObjectsConcurentWithBag<T> : IPoolObjects<T>, IPoolReturn<T>, IPoolRent<T> where T : class
    {
        private IPoolFactory<T> factory;

        private readonly ConcurrentBag<T> items = new ConcurrentBag<T>();

        public void SetFactory(IPoolFactory<T> factory)
        {
            this.factory = factory;
        }

        public void Return(T item)
        {
            items.Add(item);
        }

        public T Rent()
        {
            items.TryTake(out var result);
            return result ?? factory.Create();
        }
    }
}