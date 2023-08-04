using System.Threading.Tasks;
using IziHardGames.Proxy.WebGUI;
using Microsoft.AspNetCore.Mvc;

namespace ProxyServerWebGUI.ViewComponents
{
    public class ComponentTableDomains : ViewComponent
    {
        private GrpcConnector infoProvider;
        public ComponentTableDomains(GrpcConnector infoProvider)
        {
            this.infoProvider = infoProvider;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var domains = await infoProvider.RequestDomainsAsync().ConfigureAwait(false);
            return View(domains);
        }
    }
}
