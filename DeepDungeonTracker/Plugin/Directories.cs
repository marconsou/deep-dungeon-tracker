using System.IO;

namespace DeepDungeonTracker
{
    public static class Directories
    {
        public static string Backups => Path.Combine(ServiceUtility.ConfigDirectory, "Backups");

        public static string Screenshots => Path.Combine(ServiceUtility.ConfigDirectory, "Screenshots");
    }
}