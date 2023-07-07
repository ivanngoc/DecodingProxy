using IziHardGames.Proxy.gRPC;
using IziHardGames.Proxy.Tcp.Tls;
using IziHardGames.Proxy.WebGUI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProxyServerWebGUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly Connector connector;
        public IList<DataConnection> Connections { get; set; }

        public IndexModel(ILogger<IndexModel> logger, Connector connector)
        {
            this.connector = connector;
            _logger = logger;
            connector.Connect();

            //Connections = new List<ConnectionsToDomainTls>()
            //{
            //    new ConnectionsToDomainTls(){ AddressAndPort = "111"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "222"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "333"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "444"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "555"},
            //};
        }

        public void OnGet()
        {
            Connections = connector.GetConnections();
        }
    }
}