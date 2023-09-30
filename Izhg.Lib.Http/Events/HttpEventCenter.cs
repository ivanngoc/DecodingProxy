using System;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.States;

namespace IziHardGames.Libs.ForHttp.Monitoring
{

    public static class HttpEventCenter
    {
        private static HttpEventConsumer consumer;

        public static void SetEventConsumer(HttpEventConsumer httpEventConsumer)
        {
            HttpEventCenter.consumer = httpEventConsumer;
        }

        public static void OnNewConnection(uint idConnection)
        {
            consumer.OnNewConnectionAsync(idConnection);
        }
        public static async Task OnNewConnectionAsync(uint idConnection)
        {
            await consumer.OnNewConnectionAsync(idConnection).ConfigureAwait(false);
        }

        public static void OnAddState(uint idConnection, EHttpConnectionStates originConnected)
        {
            consumer.OnAddStateAsync(idConnection, originConnected);
        }

        internal static void OnFindHostAndPort(uint idConnection, string host, int port)
        {
            consumer.OnFindHostAndPortAsync(idConnection, host, port);
        }
    }

    public abstract class HttpEventConsumer
    {
        public const int EVENT_TYPE_NEW_CONNECTION = 1;
        public const int EVENT_TYPE_UPDATE_STATE = 2;
        public const int EVENT_TYPE_UPDATE_FLAGS = 3;
        public const int EVENT_TYPE_UPDATE_PROTOCOLS = 4;
        public const int EVENT_TYPE_SET_HOST_AND_PORT = 5;
        public const int EVENT_TYPE_LOG = 6;

        public abstract Task OnNewConnectionAsync(uint idConnection);
        public abstract Task OnFindHostAndPortAsync(uint idConnection, string host, int port);
        public abstract Task OnAddStateAsync(uint idConnection, EHttpConnectionStates originConnected);
        public abstract Task OnFlagsUpdateAsync(uint idConnection, EConnectionFlags flags);
        public abstract Task OnNetworkProtocolUpdateAsync(uint idConnection, ENetworkProtocols flags);
        public abstract Task OnLog(uint idConnection, int groupe, string message);

        public abstract void OnNewConnection(uint idConnection);
        public abstract void OnAddState(uint idConnection, EHttpConnectionStates originConnected);
        public abstract void OnFindHostAndPort(uint idConnection, string host, int port);
    }
}