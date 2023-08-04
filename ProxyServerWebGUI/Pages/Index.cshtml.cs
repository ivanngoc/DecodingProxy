using System.Collections.Generic;
using System.Linq;
using IziHardGames.Proxy.WebGUI;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using DataConnection = IziHardGames.Proxy.gRPC.ProtobufDataConnection;

namespace ProxyServerWebGUI.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly GrpcConnector connector;
        public IEnumerable<DataConnection> Connections { get; set; }

        public IndexModel(ILogger<IndexModel> logger, GrpcConnector connector)
        {
            this.connector = connector;
            _logger = logger;

            if (false) connector.Connect();

            Connections = Enumerable.Empty<DataConnection>();
            //Connections = new List<ConnectionsToDomainTls>()
            //{
            //    new ConnectionsToDomainTls(){ AddressAndPort = "111"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "222"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "333"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "444"},
            //    new ConnectionsToDomainTls(){ AddressAndPort = "555"},
            //};
        }
    }
}