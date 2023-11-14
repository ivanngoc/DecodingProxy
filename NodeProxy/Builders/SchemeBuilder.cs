namespace IziHardGames.NodeProxies
{
    public static class SchemeBuilder
    {
        internal static SchemeBuilderMonada Begin()
        {
            return new SchemeBuilderMonada();
        }
    }
}