using System;

namespace IziHardGames.Proxy
{
    public class ProxyFactory<T> where T : IProxy
    {
        private readonly Func<T> constructor;
        public static ProxyFactory<T> Shared;

        public ProxyFactory(Func<T> constructor)
        {
            this.constructor = constructor;
        }
        public static T Create(EProxyBehaviour option, Func<T> constructor)
        {
            return constructor();
        }

        public static void Init(Func<T> constructor)
        {
            Shared = new ProxyFactory<T>(constructor);
        }
    }

    public interface IProxy
    {

    }
}