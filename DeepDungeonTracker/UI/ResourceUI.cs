using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.Internal;
using Dalamud.Interface.ManagedFontAtlas;
using DeepDungeonTracker.Properties;
using System;

namespace DeepDungeonTracker;

public sealed class ResourceUI(UiBuilder uiBuilder) : IDisposable
{
    public IDalamudTextureWrap UI { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.UI);

    public IDalamudTextureWrap DeepDungeon { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.DeepDungeon);

    public IDalamudTextureWrap Job { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.Job);

    public IDalamudTextureWrap Miscellaneous { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.Miscellaneous);

    public IDalamudTextureWrap Coffer { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.Coffer);

    public IDalamudTextureWrap Enchantment { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.Enchantment);

    public IDalamudTextureWrap Trap { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.Trap);

    public IDalamudTextureWrap BossStatusTimer { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.BossStatusTimer);

    public IDalamudTextureWrap MapNormal { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapNormal);

    public IDalamudTextureWrap MapHallOfFallacies { get; } = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapHallOfFallacies);

    public IFontHandle Axis { get; } = ResourceUI.LoadFont(uiBuilder, GameFontFamily.Axis, 19.25f);

    public IFontHandle MiedingerMid { get; } = ResourceUI.LoadFont(uiBuilder, GameFontFamily.MiedingerMid, 16.0f);

    public IFontHandle MiedingerMidLarge { get; } = ResourceUI.LoadFont(uiBuilder, GameFontFamily.MiedingerMid, 22.0f);

    public IFontHandle TrumpGothic { get; } = ResourceUI.LoadFont(uiBuilder, GameFontFamily.TrumpGothic, 32.0f);

    public void Dispose()
    {
        this.UI.Dispose();
        this.DeepDungeon.Dispose();
        this.Job.Dispose();
        this.Miscellaneous.Dispose();
        this.Coffer.Dispose();
        this.Enchantment.Dispose();
        this.Trap.Dispose();
        this.BossStatusTimer.Dispose();
        this.MapNormal.Dispose();
        this.MapHallOfFallacies.Dispose();
        this.Axis.Dispose();
        this.MiedingerMid.Dispose();
        this.MiedingerMidLarge.Dispose();
        this.TrumpGothic.Dispose();
    }

    private static IFontHandle LoadFont(UiBuilder uiBuilder, GameFontFamily family, float sizePx)
    {
        return uiBuilder.FontAtlas.NewDelegateFontHandle(x => x.OnPreBuild(toolkit =>
        {
            var fontPtr = toolkit.AddGameGlyphs(new(family, sizePx), null, null);
            toolkit.SetFontScaleMode(fontPtr, FontScaleMode.UndoGlobalScale);
        }));
    }
}