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

    public TextureWrap Number { get; }

    public TextureWrap CheckMark { get; }

    public TextureWrap Job { get; }

    public TextureWrap Miscellaneous { get; }

    public TextureWrap Coffer { get; }

    public TextureWrap Enchantment { get; }

    public TextureWrap Trap { get; }

    public TextureWrap MapNormal { get; }

    public TextureWrap MapHallOfFallacies { get; }

    public TextureWrap ScreenshotButton { get; }

    public TextureWrap ArrowButton { get; }

    public TextureWrap DoubleArrowButton { get; }

    public TextureWrap CloseButton { get; }

    public TextureWrap GenericButtonOver { get; }

    public TextureWrap DivisorHorizontal { get; }

    public TextureWrap DivisorVertical { get; }

    public TextureWrap Background { get; }

    public ResourceUI()
    {
        this.Number = Service.PluginInterface.UiBuilder.LoadImage(Resources.Number);
        this.CheckMark = Service.PluginInterface.UiBuilder.LoadImage(Resources.CheckMark);
        this.Job = Service.PluginInterface.UiBuilder.LoadImage(Resources.Job);
        this.Miscellaneous = Service.PluginInterface.UiBuilder.LoadImage(Resources.Miscellaneous);
        this.Coffer = Service.PluginInterface.UiBuilder.LoadImage(Resources.Coffer);
        this.Enchantment = Service.PluginInterface.UiBuilder.LoadImage(Resources.Enchantment);
        this.Trap = Service.PluginInterface.UiBuilder.LoadImage(Resources.Trap);
        this.MapNormal = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapNormal);
        this.MapHallOfFallacies = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapHallOfFallacies);
        this.ScreenshotButton = Service.PluginInterface.UiBuilder.LoadImage(Resources.ScreenshotButton);
        this.ArrowButton = Service.PluginInterface.UiBuilder.LoadImage(Resources.ArrowButton);
        this.DoubleArrowButton = Service.PluginInterface.UiBuilder.LoadImage(Resources.DoubleArrowButton);
        this.CloseButton = Service.PluginInterface.UiBuilder.LoadImage(Resources.CloseButton);
        this.GenericButtonOver = Service.PluginInterface.UiBuilder.LoadImage(Resources.GenericButtonOver);
        this.DivisorHorizontal = Service.PluginInterface.UiBuilder.LoadImage(Resources.DivisorHorizontal);
        this.DivisorVertical = Service.PluginInterface.UiBuilder.LoadImage(Resources.DivisorVertical);
        this.Background = Service.PluginInterface.UiBuilder.LoadImage(Resources.Background);
    }

    public void BuildFonts()
    {
        var scale = 1.0f / ImGui.GetIO().FontGlobalScale;

        this.Axis = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.Axis, 19.0f * scale)).ImFont;
        this.MiedingerMid = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, 16.0f * scale)).ImFont;
        this.MiedingerMidLarge = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, 22.0f * scale)).ImFont;
        this.TrumpGothic = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.TrumpGothic, 32.0f * scale)).ImFont;
    }

    public void Dispose()
    {
        this.Number.Dispose();
        this.CheckMark.Dispose();
        this.Job.Dispose();
        this.Miscellaneous.Dispose();
        this.Coffer.Dispose();
        this.Enchantment.Dispose();
        this.Trap.Dispose();
        this.MapNormal.Dispose();
        this.MapHallOfFallacies.Dispose();
        this.ScreenshotButton.Dispose();
        this.ArrowButton.Dispose();
        this.DoubleArrowButton.Dispose();
        this.CloseButton.Dispose();
        this.GenericButtonOver.Dispose();
        this.DivisorHorizontal.Dispose();
        this.DivisorVertical.Dispose();
        this.Background.Dispose();
    }
}