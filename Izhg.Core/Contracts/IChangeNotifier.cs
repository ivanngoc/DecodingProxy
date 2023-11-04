using System;

namespace IziHardGames.Core
{

    public interface IziDisposable : IDisposable
    {
        private bool IsDisposed { get => throw new System.NotImplementedException(); set => this.IsDisposed = value; }
    }

    public interface IChangeNotifier<T>
    {
        void OnAdd(T item);
        void OnUpdate(T item);
        void OnRemove(T item);
    }
}