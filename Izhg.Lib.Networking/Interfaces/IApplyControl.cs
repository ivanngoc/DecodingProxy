// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace IziHardGames.Libs.Networking.Pipelines
{
    public interface ICheckConnection
    {
        public bool CheckConnectIndirectly();

    }

    public interface IApplyControl
    {
        void SetTimeouts(int send, int recieve);
        void SetLife(int max);
    }
}