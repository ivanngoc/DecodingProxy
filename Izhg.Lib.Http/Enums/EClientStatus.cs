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