using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace DeepDungeonTracker;

public static class NetworkStream
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<T?> Load<T>(Uri uri)
    {
        var result = await NetworkStream.HttpClient.GetAsync(uri).ConfigureAwait(true);
        return result.IsSuccessStatusCode ? await result.Content.ReadFromJsonAsync<T>().ConfigureAwait(true) : default;
    }
}