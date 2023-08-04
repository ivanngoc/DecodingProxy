using IziHardGames.Libs.Networking.Contracts;
using System;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IDomainData : IDisposable
    {
        int Id { get; set; }
        string Host { get; set; }
        int Port { get; set; }
        int Count { get; set; }
        string Status { get; set; }
        string Flags { get; set; }
    }

    public interface IConnectionData : IDisposable
    {
        string Host { get; set; }
        int Port { get; set; }
        int Id { get; set; }
        int Action { get; set; }
        string Version { get; set; }
        string Status { get; set; }
        string ToInfoConnectionData();
    }
}