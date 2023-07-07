using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp;
using System.Collections.Concurrent;

namespace IziHardGames.Libs.ObjectsManagment
{
    public class ManagerBase<TKey, TValue> : ObjectManager, IGetOrCreateUnique<TKey, TValue>, IConcurrent, IPoolReturn<TValue>
        where TValue : IDisposable
    {
        private readonly ConcurrentDictionary<TKey, TValue> keyValuePairs = new ConcurrentDictionary<TKey, TValue>();
        protected readonly Func<TKey, TValue> factory;
        public ManagerBase(Func<TKey, TValue> factory)
        {
            this.factory = factory;
        }
        public virtual TValue GetOrCreate(TKey key)
        {
            return keyValuePairs.GetOrAdd(key, factory);
        }
        public void Return(TValue value)
        {
            value.Dispose();
        }
    }

    public interface ObjectManager
    {

    }

    public interface IGetOrCreateUnique<TKey, TValue>
    {
        public TValue GetOrCreate(TKey key);
    }
    public interface IConcurrent
    {

    }
}