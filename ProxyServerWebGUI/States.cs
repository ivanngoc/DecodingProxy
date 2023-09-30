namespace IziHardGames.Proxy.WebGUI
{
    internal class States
    {
        public static bool IsInfoProviderConnected => isInfoProviderConnected;
        private static bool isInfoProviderConnected;
        internal static void SetInfoProvider()
        {
            isInfoProviderConnected = true;
        }
    }
}