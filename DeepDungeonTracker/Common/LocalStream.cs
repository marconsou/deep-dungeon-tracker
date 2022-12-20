using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeepDungeonTracker
{
    public static class LocalStream
    {
        public static async Task Save<T>(string directory, string fileName, T data)
        {
            Directory.CreateDirectory(directory);
            using FileStream fileStream = File.Create(Path.Combine(directory, fileName));
            JsonSerializerOptions options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };
            await JsonSerializer.SerializeAsync(fileStream, data, data!.GetType(), options).ConfigureAwait(true);
            await fileStream.DisposeAsync().ConfigureAwait(true);
        }

        public static T? Load<T>(string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);
            return File.Exists(path) ? JsonSerializer.Deserialize<T>(File.ReadAllText(path)) : default;
        }

        public static bool Delete(string directory, string fileName)
        {
            var path = Path.Combine(directory, fileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                return true;
            }
            return false;
        }

        public static bool Exists(string directory, string fileName) => File.Exists(Path.Combine(directory, fileName));
    }
}