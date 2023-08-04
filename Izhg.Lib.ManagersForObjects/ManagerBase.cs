using IziHardGames.Libs.NonEngine.Memory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IziHardGames.Libs.ObjectsManagment
{
    public class ManagerBase<TKey, TValue, TOptions> : IObjectManager, IGetOrCreateUnique<TKey, TValue, TOptions>, IConcurrent, IPoolReturn<TValue>
        where TValue : IKey<TKey>, IDisposable
    {
        private readonly ConcurrentDictionary<TKey, TValue> keyValuePairs = new ConcurrentDictionary<TKey, TValue>();
        protected readonly Func<TKey, TOptions, TValue> factory;

        public ManagerBase(Func<TKey, TOptions, TValue> factory)
        {
            this.factory = factory;
        }
        public virtual async Task<TValue> GetOrCreateAsync(TKey key, TOptions options, Func<TValue, Task<TValue>> initilize)
        {
            TValue value = keyValuePairs.GetOrAdd(key, (x) => factory(x, options));
            await initilize(value);
            return value;
        }
        public virtual TValue GetOrCreate(TKey key, TOptions options)
        {
            return keyValuePairs.GetOrAdd(key, (x) => factory(x, options));
        }
        public void Return(TValue value)
        {
            if (!keyValuePairs.TryRemove(new KeyValuePair<TKey, TValue>(value.Key, value)))
            {
                new SpinWait().SpinOnce();
            }
            value.Dispose();
        }
    }

    public interface IObjectManager
    {

    }

    public interface IGetOrCreateUnique<TKey, TValue, TOptions>
    {
        public TValue GetOrCreate(TKey key, TOptions options);
    }
    public interface IConcurrent
    {

    }
}