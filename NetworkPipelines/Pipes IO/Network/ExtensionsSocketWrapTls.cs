using System;
using System.Linq;
using IziHardGames.Libs.Networking.SocketLevel;
using IziHardGames.Pools.Abstractions.NetStd21;

namespace IziHardGames.Libs.Networking.Pipelines
{
    public static class ExtensionsSocketWrapTls
    {
        public static SocketWrapUpgradeTlsHttp11 FindTlsUpgrade(this SocketWrap socketWrap)
        {
            return (socketWrap.upgrades.First(x => x is SocketWrapUpgradeTlsHttp11) as SocketWrapUpgradeTlsHttp11) ?? throw new NullReferenceException($"Upgrade of type [{typeof(SocketWrapUpgradeTlsHttp11).FullName}] Not Founded");
        }
        public static void UpgradeTls(this SocketWrap socketWrap)
        {
            var pool = PoolObjectsConcurent<SocketWrapUpgradeTlsHttp11>.Shared;
            SocketWrapUpgradeTlsHttp11 upgrade = pool.Rent();
            upgrade.BindToPool(pool);
            upgrade.Initilize(socketWrap);
            socketWrap.AddUpgrade(upgrade);
        }
    }
}