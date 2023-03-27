using ImGuiScene;
using System;
using System.IO;
using System.Text.Json;

namespace DeepDungeonTracker;

public sealed class Font : IDisposable
{
    public FontLayout FontLayout { get; }

    public TextureWrap TextureAtlas { get; }

    public void Dispose() => this.TextureAtlas.Dispose();

    public Font(byte[] fontResource, byte[] textureResource)
    {
        using var memoryStream = new MemoryStream(fontResource);
        using var streamReader = new StreamReader(memoryStream);
        this.FontLayout = JsonSerializer.Deserialize<FontLayout>(streamReader.ReadToEnd(), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
        this.TextureAtlas = Service.PluginInterface.UiBuilder.LoadImage(textureResource);
    }
}