using System;

namespace IziHardGames.Libs.Networking.Options
{
    public class NetworkConstants
    {
        public class Reading
        {
            public const int STATUS_CONSISTANT = 1;
            public const int STATUS_PARTIAL = 2;
        }
    }

    [Flags]
    public enum EReadStatus
    {
        None = 0,
        Complete = 1 << 1,
        Canceled = 1 << 2,
        /// <summary>
        /// there is no Availavle bytes on socket
        /// </summary>
        AwaitingBytes = 1 << 3,
        /// <summary>
        /// Partial Read With Integrity Checking = Not Completed
        /// </summary>
        PartialIncomplete = 1 << 4,
        /// <summary>
        /// Partial Read With Integrity Checking = Completed
        /// </summary>
        PartialComplete = 1 << 5,
        /// <summary>
        /// Partial Read Without Integrity Checking
        /// </summary>
        PartialUnchecked = 1 << 6,
        /// <summary>
        /// Block Until Further Data Recived. Обычно используется при конвейерной обработке данных
        /// </summary>
        Blocking = 1 << 7,
        /// <summary>
        /// Skip and exit from reading method
        /// </summary>
        Passing = 1 << 8,
    }
}