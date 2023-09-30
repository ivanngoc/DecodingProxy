using System.Linq;
using System.Threading.Tasks;
using IziHardGames.Libs.Networking.Contracts;
using IziHardGames.Proxy.WebGUI;
using Microsoft.AspNetCore.Mvc;

namespace ProxyServerWebGUI.ViewComponents
{
    public class ComponentTableDomains : ViewComponent
    {
        private ProxyChangeReflector proxyChangeReflector;
        private ProxyInfoProvider infoProvider;
        public ComponentTableDomains(ProxyInfoProvider infoProvider, ProxyChangeReflector proxyChangeReflector)
        {
            this.proxyChangeReflector = proxyChangeReflector;
            this.infoProvider = infoProvider;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            //var domains = await infoProvider.RequestDomainsAsync().ConfigureAwait(false);
            ////return View(domains);
            return View(Enumerable.Empty<IDomainData>());
        }
    }
}
