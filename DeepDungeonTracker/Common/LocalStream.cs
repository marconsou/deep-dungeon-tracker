using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace DeepDungeonTracker
{
    public static class LocalStream
    {
        public static async Task Save<T>(string directory, string fileName, T data)
        {
            Directory.CreateDirectory(directory);
            using FileStream fileStream = File.Create(Path.Combine(directory, fileName));
            await JsonSerializer.SerializeAsync(fileStream, data);
            await fileStream.DisposeAsync();
        }

        public static T? Load<T>(string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);
            return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : default;
        }

        public static void Delete(string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        public static bool Exists(string directory, string fileName) => File.Exists(Path.Combine(directory, fileName));
    }
}