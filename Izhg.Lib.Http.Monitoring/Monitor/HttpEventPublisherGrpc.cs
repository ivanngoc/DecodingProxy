using System.Text;
using System.Text.Json.Nodes;
using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.gRPC.Services;
using IziHardGames.Libs.Networking.States;

namespace IziHardGames.Libs.ForHttp.Monitoring
{
    public class HttpEventPublisherGrpc : HttpEventConsumer, IDisposable
    {
        public static readonly ManagerForInfoConnections connections = new ManagerForInfoConnections();
        private readonly GrpcHubService grpcHubService;
        public HttpEventPublisherGrpc(GrpcHubService grpcHubService)
        {
            this.grpcHubService = grpcHubService;
        }

        public override async Task OnNewConnectionAsync(uint idConnection)
        {
            var json = new JsonObject()
            {
                ["idConnection"] = idConnection,
                ["event"] = HttpEventConsumer.EVENT_TYPE_NEW_CONNECTION,
            };
            var t1 = this.SendObjectAsync(ConstantsForHttp.Events.TYPE_ACTION_HANDLE_EVENT, Encoding.UTF8.GetBytes(json.ToJsonString()));
            connections.Create(idConnection);
            await t1.ConfigureAwait(false);
        }

        private async Task SendObjectAsync(int action, byte[] bytes)
        {
            if (grpcHubService != null)
            {
                await grpcHubService.SendToSubscribersAsync(action, bytes);
            }
        }

        public override async Task OnAddStateAsync(uint idConnection, EHttpConnectionStates originConnected)
        {
            var json = new JsonObject()
            {
                ["idConnection"] = idConnection,
                ["event"] = HttpEventConsumer.EVENT_TYPE_UPDATE_STATE,
                ["state"] = (int)originConnected,
            };
            var t1 = this.SendObjectAsync(ConstantsForHttp.Events.TYPE_ACTION_HANDLE_EVENT, Encoding.UTF8.GetBytes(json.ToJsonString()));
            connections.AddState(idConnection, originConnected);
            await t1.ConfigureAwait(false);
        }

        public override async Task OnFindHostAndPortAsync(uint idConnection, string host, int port)
        {
            var json = new JsonObject()
            {
                ["idConnection"] = idConnection,
                ["event"] = HttpEventConsumer.EVENT_TYPE_SET_HOST_AND_PORT,
                ["host"] = host,
                ["port"] = port,
            };
            var t1 = this.SendObjectAsync(ConstantsForHttp.Events.TYPE_ACTION_HANDLE_EVENT, Encoding.UTF8.GetBytes(json.ToJsonString()));
            connections.SetHostAndPort(idConnection, host, port);
            await t1.ConfigureAwait(false);
        }
        public override async Task OnFlagsUpdateAsync(uint idConnection, EConnectionFlags flags)
        {
            var json = new JsonObject()
            {
                ["idConnection"] = idConnection,
                ["event"] = HttpEventConsumer.EVENT_TYPE_UPDATE_FLAGS,
                ["flags"] = (int)flags,
            };
            var t1 = this.SendObjectAsync(ConstantsForHttp.Events.TYPE_ACTION_HANDLE_EVENT, Encoding.UTF8.GetBytes(json.ToJsonString()));
            connections.UpdateFlags(idConnection, flags);
            await t1.ConfigureAwait(false);
        }

        public override async Task OnNetworkProtocolUpdateAsync(uint idConnection, ENetworkProtocols protocols)
        {
            var json = new JsonObject()
            {
                ["idConnection"] = idConnection,
                ["event"] = HttpEventConsumer.EVENT_TYPE_UPDATE_PROTOCOLS,
                ["protocols"] = (int)protocols,
            };
            var t1 = this.SendObjectAsync(ConstantsForHttp.Events.TYPE_ACTION_HANDLE_EVENT, Encoding.UTF8.GetBytes(json.ToJsonString()));
            connections.UpdateProtocols(idConnection, protocols);
            await t1.ConfigureAwait(false);
        }
        public override async Task OnLog(uint idConnection, int groupe, string message)
        {
            var json = new JsonObject()
            {
                ["idConnection"] = idConnection,
                ["event"] = HttpEventConsumer.EVENT_TYPE_LOG,
                ["message"] = message,
                ["groupe"] = groupe,
            };
            var t1 = this.SendObjectAsync(ConstantsForHttp.Events.TYPE_ACTION_HANDLE_EVENT, Encoding.UTF8.GetBytes(json.ToJsonString()));
            connections.AddLog(idConnection, groupe, message);
            await t1.ConfigureAwait(false);
        }

        public override void OnNewConnection(uint idConnection)
        {
            throw new NotImplementedException();
        }

        public override void OnAddState(uint idConnection, EHttpConnectionStates originConnected)
        {
            throw new NotImplementedException();
        }

        public override void OnFindHostAndPort(uint idConnection, string host, int port)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}