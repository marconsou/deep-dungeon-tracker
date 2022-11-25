namespace DeepDungeonTracker
{
    public static class ServiceUtility
    {
        public static bool IsSolo => Service.PartyList.Length <= 1;

        public static string ConfigDirectory => Service.PluginInterface.ConfigDirectory.FullName;
    }
}