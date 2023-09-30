using System.Threading.Tasks;
using Grpc.Net.Client;
using IziHardGames.Libs.gRPC.InterprocessCommunication;
using Microsoft.Extensions.Logging;

namespace IziHardGames.Proxy.WebGUI
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// <see cref="ProxyChangeReflector"/> - тоже подключается через gRPC используя другой сервис. Сам он настраивает <see cref="Libs.gRPC.Services.GrpcHubService"/>
    /// который является сервером.
    /// Клиент на другой стороне - <see cref="IziHardGames.Libs.gRPC.InterprocessCommunication.BidirectionalClient"/>
    /// 
    /// В начальной версии сервером был преокт прокси, а webGUI подключался к нему за данными.
    /// <see cref="ProxyChangeReflector"/> напротив сам является сервером который запрашивает данные
    /// </remarks>
    public class ProxyInfoProvider
    {
        private string address = "http://localhost:5104";
        private readonly ProxyChangeReflector reflector;
        private ILogger<ProxyInfoProvider> logger;
        private Task task;
        private GrpcChannel grpcChannel;
        private BidirectionalClient client;

        public ProxyInfoProvider(ILogger<ProxyInfoProvider> logger, ProxyChangeReflector reflector)
        {
            this.logger = logger;
            this.reflector = reflector;
        }
        public void ConnectAsync()
        {
            this.grpcChannel = GrpcChannel.ForAddress(address);
            this.client = new BidirectionalClient("WebGui Info Provider");
            client.Connect(address);
        }       
    }
}