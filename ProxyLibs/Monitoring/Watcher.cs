using IziHardGames.Core;

namespace IziHardGames.Monitoring
{
    public class ConstantsMonitoring
    {
        public const int ACTION_ADD = 1;
        public const int ACTION_REMOVE = 2;
        public const int ACTION_UPDATE = 3;
        public const int ACTION_UPDATE_STATUS = 4;
    }

    public class Watcher<T>
    {
        public void NotifyAdd(T item)
        {

        }
    }
    public class WatcherGrpc<T> : Watcher<T>
    {
        private IGrpcNotifier<T> notifier;
    }



    public interface IGrpcNotifier<T> : IChangeNotifier<T>
    {

    }
}