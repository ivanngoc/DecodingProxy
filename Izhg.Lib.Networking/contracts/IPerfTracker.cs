using System;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IPerfTracker
    {
        void ReportTime(string text);
        void PutMsg(object msg);
    }
}