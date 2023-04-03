using Dalamud.Interface.GameFonts;
using DeepDungeonTracker.Properties;
using ImGuiNET;
using ImGuiScene;
using System;

namespace DeepDungeonTracker;

public sealed class ResourceUI : IDisposable
{
    public ImFontPtr Axis { get; set; }

    public ImFontPtr MiedingerMid { get; set; }

    public ImFontPtr MiedingerMidLarge { get; set; }

    public ImFontPtr TrumpGothic { get; set; }

    public TextureWrap UI { get; }

    public TextureWrap DeepDungeon { get; }

    public TextureWrap Job { get; }

    public TextureWrap Miscellaneous { get; }

    public TextureWrap Coffer { get; }

    public TextureWrap Enchantment { get; }

    public TextureWrap Trap { get; }

    public TextureWrap MapNormal { get; }

    public TextureWrap MapHallOfFallacies { get; }

    public ResourceUI()
    {
        this.UI = Service.PluginInterface.UiBuilder.LoadImage(Resources.UI);
        this.DeepDungeon = Service.PluginInterface.UiBuilder.LoadImage(Resources.DeepDungeon);
        this.Job = Service.PluginInterface.UiBuilder.LoadImage(Resources.Job);
        this.Miscellaneous = Service.PluginInterface.UiBuilder.LoadImage(Resources.Miscellaneous);
        this.Coffer = Service.PluginInterface.UiBuilder.LoadImage(Resources.Coffer);
        this.Enchantment = Service.PluginInterface.UiBuilder.LoadImage(Resources.Enchantment);
        this.Trap = Service.PluginInterface.UiBuilder.LoadImage(Resources.Trap);
        this.MapNormal = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapNormal);
        this.MapHallOfFallacies = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapHallOfFallacies);
    }

    public void BuildFonts()
    {
        var scale = 1.0f / ImGui.GetIO().FontGlobalScale;
        this.Axis = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 19.25f * scale)).ImFont;
        this.MiedingerMid = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, 16.0f * scale)).ImFont;
        this.MiedingerMidLarge = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, 22.0f * scale)).ImFont;
        this.TrumpGothic = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.TrumpGothic, 32.0f * scale)).ImFont;
    }

    public void Dispose()
    {
        this.UI.Dispose();
        this.DeepDungeon.Dispose();
        this.Job.Dispose();
        this.Miscellaneous.Dispose();
        this.Coffer.Dispose();
        this.Enchantment.Dispose();
        this.Trap.Dispose();
        this.MapNormal.Dispose();
        this.MapHallOfFallacies.Dispose();
    }
}