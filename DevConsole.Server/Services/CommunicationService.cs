using System;
using System.Threading;
using System.Threading.Tasks;
using DevConsole.Server.Protos;
using Grpc.Core;
using Izhg.Libs.gRPC.Shared;
using Func = System.Func<DevConsole.Server.Protos.ProtobufMsg, DevConsole.Server.Protos.ProtobufMsg>;

namespace DevConsole.Server.Services
{
    public class CommunicationService : DevConsoleService.DevConsoleServiceBase
    {
        private CancellationTokenSource cts;
        private Func? func;
        public async override Task MaintainConnection(IAsyncStreamReader<ProtobufMsg> requestStream, IServerStreamWriter<ProtobufMsg> responseStream, ServerCallContext context)
        {
            if (this.func is null) throw new NullReferenceException("Func is not set");
            cts = new CancellationTokenSource();
            await GrpcUtilForServers.HandleBidirectional(requestStream, responseStream, context, func!, cts.Token).ConfigureAwait(false);
        }
        public void RegistHandler(Func func)
        {
            this.func = func;
        }
    }
}
