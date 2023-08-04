using System;
using System.Threading;

namespace IziHardGames.Proxy.Sniffing.ForHttp
{
    public class SyncRedirection : IDisposable
    {
        public int syncMsgCountRecieve;
        public int syncMsgCountResend;
        public bool isRecievingClientToOriginCompleted;
        public bool isRecievingOriginToClientCompleted;
        public bool isResendingClientToOriginCompleted;
        public bool isResendingOriginToClientCompleted;

        public void Dispose()
        {
            isRecievingClientToOriginCompleted = default;
            isRecievingOriginToClientCompleted = default;

            isResendingClientToOriginCompleted = default;
            isResendingOriginToClientCompleted = default;

            syncMsgCountRecieve = default;
            syncMsgCountResend = default;
        }
        public int IncrementRecieve()
        {
            return Interlocked.Increment(ref syncMsgCountRecieve);
        }
        public int IncrementResend()
        {
            return Interlocked.Increment(ref syncMsgCountResend);
        }
        public int DecrementRecieve()
        {
            return Interlocked.Decrement(ref syncMsgCountRecieve);
        }
        public int DecrementResend()
        {
            return Interlocked.Decrement(ref syncMsgCountResend);
        }
    }
}