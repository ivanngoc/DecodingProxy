// See https://aka.ms/new-console-template for more information
//SslTcpProxy.Test();

namespace IziHardGames.Proxy
{
    public enum EClientStatus
    {
        None,
        Initilized,
        AwaitRequestFromAgent,
        SendingRequestToOrigin,
        ReadingResponseFromOrigin,
        CopyResponseToAgent,
        Complete,
    }
}