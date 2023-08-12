using System.Net.Security;

namespace IziHardGames.Libs.Networking.Contracts
{
    public interface IUpgradableConnection<T>
    {
        T UpgradeTls(SslClientAuthenticationOptions options);
    }
}