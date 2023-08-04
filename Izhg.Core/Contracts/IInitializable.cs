using System.Runtime.CompilerServices;

namespace IziHardGames.Core
{
    /// <summary>
    /// Type with implemented Initilize() should cast initData
    /// </summary>
    public interface IInitializableFlex
    {
        void Initilize<T>(T initData);
    }
    public interface IInitializable
    {
        void Initilize();
    }
    public interface IInitializable<in T>
    {
        void Initilize(T t);
    }
    public interface IInitializable<T1, T2>
    {
        void Initilize(T1 t1, T2 t2);
    }
}