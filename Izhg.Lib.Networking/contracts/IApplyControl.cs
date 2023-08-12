namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IApplyControl
    {
        void SetTimeouts(int send, int recieve);
        void SetLife(int max);
    }
}