using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using IziHardGames.Libs.ForHttp.Monitoring;
using IziHardGames.Libs.gRPC.Services;
using IziHardGames.Libs.Networking.Contracts;
using ProxyServerWebGUI.Hubs;
using ProxyServerWebGUI.Workers;
using Func = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<System.ReadOnlyMemory<byte>>>;

namespace IziHardGames.Proxy.WebGUI
{
    public class ProxyChangeReflector
    {
        private readonly SignalRInfoService signalR;
        private readonly SignalRInfoHub hub;

        private readonly GrpcHubService grpcHubService;
        private readonly Action<IConnectionData>[] actions;
        public ProxyChangeReflector(SignalRInfoHub hub, SignalRInfoService signalRInfoService, GrpcHubService grpcHubService)
        {
            this.hub = hub;
            this.grpcHubService = grpcHubService;
            this.signalR = signalRInfoService;

            actions = new Action<IConnectionData>[]
            {
                (x)=>{throw new ArgumentOutOfRangeException("Action is 0"); },
                (x)=> signalRInfoService.AddForAll(x),               /// <see cref="Monitoring.ConstantsMonitoring.ACTION_ADD"/>
                (x)=> signalRInfoService.RemoveForAll(x),            /// <see cref="Monitoring.ConstantsMonitoring.ACTION_REMOVE"/>
                (x)=> signalRInfoService.UpdateForAll(x),            /// <see cref="Monitoring.ConstantsMonitoring.ACTION_UPDATE"/>
                (x)=> signalRInfoService.UpdateForAllStatus(x),      /// <see cref="Monitoring.ConstantsMonitoring.ACTION_UPDATE"/>
            };

            grpcHubService.SetHandlers(new Func[] {
                (x)=> throw new  ArgumentOutOfRangeException("This index Processed as Error. Index Must be Non Zero"),
                HandleEvent, /// <see cref="HttpDecodingProxy.ForHttp.ConstantsForHttp.Events"/>
                RegistInfoProvider , /// <see cref="IziHardGames.Libs.ForHttp.Monitoring.ConstantsForMonitoring.ACTION_MARK_AS_INFO_PROVIDER"/>
            });
        }

        private ValueTask<ReadOnlyMemory<byte>> RegistInfoProvider(ReadOnlyMemory<byte> arg)
        {
            States.SetInfoProvider();
            return new ValueTask<ReadOnlyMemory<byte>>();
        }

        private async ValueTask<ReadOnlyMemory<byte>> HandleEvent(ReadOnlyMemory<byte> arg)
        {
            JsonObject json = JsonNode.Parse(arg.Span)?.AsObject() ?? throw new NullReferenceException();
            var eventType = (int)json["event"];

            switch (eventType)
            {
                case HttpEventConsumer.EVENT_TYPE_NEW_CONNECTION:
                    {
                        int idConnection = (int)json["idConnection"]!;
                        await signalR.AddForAll(idConnection);
                        break;
                    }
                default: throw new System.NotImplementedException(eventType.ToString());
            }
            return default;
        }

        public void Recieve(IConnectionData data)
        {
            actions[data.Action].Invoke(data);
        }
    }
}