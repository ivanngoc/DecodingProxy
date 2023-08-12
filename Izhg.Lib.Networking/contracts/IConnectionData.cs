using IziHardGames.Libs.Networking.Contracts;
using System;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IDomainData : IDisposable
    {
        /// <summary>
        /// Inner ID of hub asigned by server 
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// Target Host
        /// </summary>
        string Host { get; set; }
        /// <summary>
        /// Target Port
        /// </summary>
        int Port { get; set; }
        /// <summary>
        /// Count of Active Connections
        /// </summary>
        int Count { get; set; }
        string Status { get; set; }
        string Flags { get; set; }
    }

    public interface IGetConnectionData<T> where T : IConnectionData
    {
        T ConnectionData { get; }
    }

    public interface IConnectionData : IDisposable
    {
        string Host { get; set; }
        int Port { get; set; }
        int Id { get; set; }
        /// <summary>
        /// Action For recepient of this Data
        /// </summary>
        int Action { get; set; }
        string Version { get; set; }
        string Status { get; set; }
        string ToInfoConnectionData();
    }
}