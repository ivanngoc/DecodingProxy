namespace IziHardGames.Libs.NonEngine.Memory
{
    public class DefaultFactory<T> : IFactory<T> where T : new()
    {
        public T Create()
        {
            return new T();
        }
    }
}