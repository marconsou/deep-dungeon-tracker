using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DeepDungeonTracker;

public static class LocalStream
{
    private static JsonSerializerOptions Options => new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };

    public static async Task Save<T>(string directory, string fileName, T data)
    {
        Directory.CreateDirectory(directory);
        using FileStream fileStream = File.Create(Path.Combine(directory, fileName));
        await JsonSerializer.SerializeAsync(fileStream, data, data!.GetType(), LocalStream.Options).ConfigureAwait(true);
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

    public static bool Exists(string directory) => Directory.Exists(directory);

    public static bool Exists(string directory, string fileName) => File.Exists(Path.Combine(directory, fileName));

    public static bool Copy(string sourceDirectory, string destDirectory, string sourceFileName, string destFileName)
    {
        if (!LocalStream.Exists(sourceDirectory))
            Directory.CreateDirectory(sourceDirectory);

        if (!LocalStream.Exists(destDirectory))
            Directory.CreateDirectory(destDirectory);

        if (LocalStream.Exists(sourceDirectory, sourceFileName))
        {
            File.Copy(Path.Combine(sourceDirectory, sourceFileName), Path.Combine(destDirectory, destFileName), true);
            return true;
        }
        return false;
    }

    public static bool Move(string sourceDirectory, string destDirectory, string sourceFileName, string destFileName)
    {
        if (!LocalStream.Exists(sourceDirectory))
            Directory.CreateDirectory(sourceDirectory);

        if (!LocalStream.Exists(destDirectory))
            Directory.CreateDirectory(destDirectory);

        if (LocalStream.Exists(sourceDirectory, sourceFileName))
        {
            File.Move(Path.Combine(sourceDirectory, sourceFileName), Path.Combine(destDirectory, destFileName), true);
            return true;
        }
        return false;
    }

    public static string[] GetFileNamesFromDirectory(string directory) => LocalStream.Exists(directory) ? Directory.EnumerateFiles(directory).ToArray() : [];

    public static void OpenFolder(string directory)
    {
        if (!LocalStream.Exists(directory))
            Directory.CreateDirectory(directory);

        Process.Start(new ProcessStartInfo { FileName = directory, UseShellExecute = true });
    }

    public static string FormatFileName(string fileName, bool includeExtension) => includeExtension ? Path.GetFileName(fileName) : Path.GetFileNameWithoutExtension(fileName);

    public static bool IsExtension(string filename, string extension) => Path.GetExtension(filename).Equals(extension, StringComparison.OrdinalIgnoreCase);
}