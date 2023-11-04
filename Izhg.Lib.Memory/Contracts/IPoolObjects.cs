using System;

namespace IziHardGames.Libs.NonEngine.Memory
{
    public interface IGuid
    {
        Guid Guid { get; }
    }

    public interface IKey<T>
    {
        T Key { get; set; }
    }

    public interface IPoolItem<T>
    {
        IPoolObjects<T> PoolObjects { get; set; }
    }

    public interface IPoolObjects
    {

    }

    /// <summary>
    /// Rent/Return System.Object
    /// </summary>
    public interface IPoolObjectsNoTyped : IPoolObjects, IPoolRent, IPoolReturn
    {

    }

    public interface IPoolObjects<T> : IPoolRent<T>, IPoolReturn<T>, IPoolObjects
    {

    }

    public interface IPoolRent
    {
        public object Rent();
    }
    public interface IPoolReturn
    {
        public void Return(object o);
    }

    // Covariance permits a method to have a more derived return type than that defined by the generic type parameter of the interface.
    // IEnumerable<Object> = IEnumerable<String>;
    public interface IPoolRent<out T>
    {
        T Rent();
    }


    // Contravariance permits a method to have argument types that are less derived than that specified by the generic parameter of the interface.
    // IEqualityComparer<DerivedClass> = IEqualityComparer<BaseClass>;
    public interface IPoolReturn<in T>
    {
        void Return(T item);
    }
    public interface IPoolBind
    {
        void Bind(IPoolObjects pool);
    }

    public interface IPoolBindTrait<T> where T : class, new()
    {
        public IPoolReturn<T> Pool { get; set; }
        public void BindToPool(IPoolReturn<T> pool)
        {
            this.Pool = pool;
        }
    }

    public interface IPoolBind<T>
    {
        void BindToPool(IPoolReturn<T> pool);
    }

    public interface IPooDisposable<T> : IDisposable, IPoolBindTrait<T> where T : class, new()
    {
        void DisposeAndReturnToPool()
        {

            //T item = this;
            //this.Dispose();
            //Pool.Return(this);
        }
    }
}