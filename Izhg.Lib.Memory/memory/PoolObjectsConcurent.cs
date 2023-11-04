using System;

namespace IziHardGames.Libs.NonEngine.Memory
{
    public class PoolObjectsConcurent<T>
        where T : class, new()
    {
        public static IPoolReturn<T> ForReturn => Shared as IPoolReturn<T> ?? throw new NullReferenceException();
        public static IPoolRent<T> ForRent => Shared as IPoolRent<T> ?? throw new NullReferenceException();
        public static IPoolObjects<T> Shared => GetOrCreate();
        private static IPoolObjects<T> shared;

        private readonly static object lockPool = new object();

        public static IPoolObjects<T> GetOrCreate()
        {
            if (shared == null)
            {
                lock (lockPool)
                {
                    if (shared == null)
                    {
                        var newPool = new PoolObjectsConcurentWithBag<T>();
                        newPool.SetFactory(new DefaultFactory<T>());
                        shared = newPool;
                    }
                }
            }
            return shared;
        }
    }


    public static class PoolUtil
    {
        public static T Rent<T>() where T : class, IDisposable, IPoolBindTrait<T>, new()
        {
            var pool = PoolObjectsConcurent<T>.Shared;
            var item = pool.Rent();
            item.BindToPool(pool);
            return item;
        }
    }
}