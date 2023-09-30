using System.Linq;
using System.Threading.Tasks;
using IziHardGames.Libs.ForHttp.Monitoring;
using IziHardGames.Proxy.WebGUI;
using Microsoft.AspNetCore.Mvc;

namespace ProxyServerWebGUI.ViewComponents
{
    public class ComponentTableConnections : ViewComponent
    {
        private ProxyInfoProvider infoProvider;
        private ProxyChangeReflector proxyChangeReflector;

        public ComponentTableConnections(ProxyInfoProvider infoProvider, ProxyChangeReflector proxyChangeReflector)
        {
            this.infoProvider = infoProvider;
            this.proxyChangeReflector = proxyChangeReflector;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //var connections = await infoProvider.RequestConnectionsAsync().ConfigureAwait(false);
            //return View(connections);

            return View(Enumerable.Empty<InfoConnection>());
        }
    }
}
