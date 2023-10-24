using System.Threading.Tasks;
using DevConsole.Client.CompareWindow;
using DevConsole.Shared.Consoles;
using IziHardGames.MappedFrameReader;
#pragma warning disable


namespace IziHardGames.Tests
{
    public class DevConsoleControl
    {
        private CompareWindowClient compareWindowControl;
        private ConsoleClient consoleClient;

        public DevConsoleControl()
        {

        }

        public async Task Start()
        {
            ConsoleClient consoleClient = new ConsoleClient();
            this.consoleClient = consoleClient;
            var logger = await consoleClient.Start();
            this.compareWindowControl = await consoleClient.NewCompareWindowAsync().ConfigureAwait(false);
        }
        public async Task SendToServer(string channelId, NodeResult result)
        {
            throw new System.NotImplementedException();
        }
    }
}