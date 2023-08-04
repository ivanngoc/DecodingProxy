using System.Threading.Tasks;
using IziHardGames.Proxy.WebGUI;
using Microsoft.AspNetCore.Mvc;
using DataConnection = IziHardGames.Proxy.gRPC.ProtobufDataConnection;
using Reply = IziHardGames.Proxy.gRPC.ProtobufReply;
using Request = IziHardGames.Proxy.gRPC.ProtobufRequest;

namespace ProxyServerWebGUI.ViewComponents
{
    public class ComponentTableConnections : ViewComponent
    {
        private GrpcConnector infoProvider;
        public ComponentTableConnections(GrpcConnector infoProvider)
        {
            this.infoProvider = infoProvider;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var connections = await infoProvider.RequestConnectionsAsync().ConfigureAwait(false);
            return View(connections);
        }
    }
}
