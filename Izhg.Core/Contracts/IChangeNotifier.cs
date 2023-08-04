namespace IziHardGames.Core
{

    public interface IChangeNotifier<T>
    {
        void OnAdd(T item);
        void OnUpdate(T item);
        void OnRemove(T item);
    }
}