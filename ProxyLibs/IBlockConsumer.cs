// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

using IziHardGames.Proxy.Datas;

namespace IziHardGames.Proxy
{
    public interface IDataConsumerForRequestResponse
    {
        void ConsumeRequest(Block data);
        void ConsumeResponse(Block data);
    }

    public interface IBlockConsumer
    {
        void Consume(Block data);
    }
}