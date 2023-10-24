using System.Threading;
using System.Threading.Tasks;
using DevConsole.Shared.Consoles;
using Microsoft.Extensions.Hosting;

namespace DevConsole.Server.Services
{
    internal class ConsolesServerService : BackgroundService
    {
        private readonly ConsolesManager manager;
        private readonly ConsolesServer server;
        private readonly ConsoleService realTimeService;

        public ConsolesServerService(ConsolesManager manager, ConsolesServer consolesServer, ConsoleService realTimeService)
        {
            this.server = consolesServer;
            this.manager = manager;
            this.realTimeService = realTimeService;
        }
        protected override Task ExecuteAsync(CancellationToken ct)
        {
            return server.StartServer(realTimeService.funcStart, realTimeService.actionUpdate, ct);
        }
    }
}
