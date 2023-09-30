using IziHardGames.Libs.Networking.States;

namespace IziHardGames.Libs.Networking.Contracts
{

    public interface IConnection
    {

    }

    public interface IConnection<T> : IGetConnectionData<T>, IConnection
        where T : IConnectionData
    {
        public void SetState(EConnectionState state);
    }
}