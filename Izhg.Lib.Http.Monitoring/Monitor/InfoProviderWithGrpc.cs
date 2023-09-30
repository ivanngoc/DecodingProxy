using IziHardGames.Libs.gRPC.InterprocessCommunication;
using IziHardGames.Libs.Networking.Contracts;
using Func = System.Func<System.ReadOnlyMemory<byte>, System.Threading.Tasks.ValueTask<System.ReadOnlyMemory<byte>>>;

namespace IziHardGames.Libs.ForHttp.Monitoring
{
    public class InfoProviderWithGrpc
    {
        private readonly ManagerForInfoConnections manger;
        //private readonly BidirectionalClient client;

        public InfoProviderWithGrpc(ManagerForInfoConnections manager)
        {
            this.manger = manager;
            //client = new BidirectionalClient("Info Provider");

            //Func[] funcs = new Func[] {
            //  (x)=> throw new InvalidOperationException("Index Reserved. Any calls will handled as errors"),
            //  SendConnections,  /// <see cref="ConstantsForMonitoring.ACTION_MARK_AS_INFO_PROVIDER"/>
            //};
            //client.SetFuncs(funcs);
        }

        private async ValueTask<ReadOnlyMemory<byte>> SendConnections(ReadOnlyMemory<byte> arg)
        {
            throw new NotImplementedException();
        }

        public async Task Run(CancellationToken ct = default)
        {
            //await client.ConnectForServerRequestsAsync(ConstantsForMonitoring.gRPC).ConfigureAwait(false);

            //while (!ct.IsCancellationRequested)
            //{
            //    await client.RecieveAndHandleAsync(ct).ConfigureAwait(false);
            //}
        }
    }
}