namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IHubBind<THub>
    {
        void BindToHub(THub hub);
    }
}