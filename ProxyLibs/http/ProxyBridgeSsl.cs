using HttpDecodingProxy.ForHttp;
using IziHardGames.Libs.NonEngine.Memory;
using IziHardGames.Proxy.Tcp.Tls;
using ProxyLibs.Extensions;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class ProxyBridgeSsl : ProxyBridge
    {
        public override void InitToReuse(ConnectionsToDomainTls ctd, StartOptions startOptions)
        {
            base.InitToReuse(ctd, startOptions);
        }

        public override void Dispose()
        {
            base.Dispose();
            PoolObjects<ProxyBridgeSsl>.Shared.Return(this);
        }

        public override async Task MakeInterceptingBridge(ConnectionToOriginTls cto, ConnectionToAgentTls cta)
        {
            StartOptions options = startOptions;

            await base.MakeInterceptingBridge(cto, cta).ConfigureAwait(false);

            if (options.IsConnectRequired)
            {
                cta.FinishConectMethod();
                Logger.LogLine($"Finished CONNECT method to {cta.Host}");
                await cta.ForgeSslConnection().ConfigureAwait(false);
            }
        }
    }
}