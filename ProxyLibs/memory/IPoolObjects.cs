namespace IziHardGames.Libs.NonEngine.Memory
{
    public interface IPoolItem<T>
    {
        IPoolObjects<T> PoolObjects { get; set; }
    }

    public interface IPoolObjects<T> : IPoolRent<T>, IPoolReturn<T>
    {

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
}