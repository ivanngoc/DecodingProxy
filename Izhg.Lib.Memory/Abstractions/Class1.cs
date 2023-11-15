using System;
using System.Collections.Generic;
using System.Text;
using IziHardGames.Libs.NonEngine.Memory;

namespace IziHardGames.ObjectPools.Abstractions.Lib.Abstractions
{
    public static class IziPool
    {
        public static readonly FactorySelector factories = new FactorySelector();
        public static T GetConcurrent<T>() where T : class, new()
        {
            return PoolObjectsConcurent<T>.Shared.Rent();
        }
        public static void ReturnConcurrent<T>(T obj) where T : class, new()
        {
            PoolObjectsConcurent<T>.Shared.Return(obj);
        }
    }

    public class FactorySelector
    {
        private readonly Dictionary<Type, PoolObjectFactoryAbstract> pairs = new Dictionary<Type, PoolObjectFactoryAbstract>();
        public PoolObjectFactoryAbstract this[Type type] { get => pairs[type]; set => AddOrUpdate(type, value); }
        private void AddOrUpdate(Type type, PoolObjectFactoryAbstract value)
        {
            if (pairs.TryGetValue(type, out var existed))
            {
                pairs[type] = value;
            }
            else
            {
                pairs.Add(type, value);
            }
        }
    }

    public abstract class PoolObjectFactoryAbstract
    {
        public readonly Func<object> create;
        public PoolObjectFactoryAbstract()
        {

        }
    }
}
