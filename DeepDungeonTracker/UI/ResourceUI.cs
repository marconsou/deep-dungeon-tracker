using Dalamud.Interface;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Textures.TextureWraps;
using DeepDungeonTracker.Properties;
using System;

namespace DeepDungeonTracker;

public sealed class ResourceUI(IUiBuilder uiBuilder) : IDisposable
{
    public IDalamudTextureWrap UI { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.UI).Result;

    public IDalamudTextureWrap DeepDungeon { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.DeepDungeon).Result;

    public IDalamudTextureWrap Job { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.Job).Result;

    public IDalamudTextureWrap Miscellaneous { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.Miscellaneous).Result;

    public IDalamudTextureWrap Coffer { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.Coffer).Result;

    public IDalamudTextureWrap Enchantment { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.Enchantment).Result;

    public IDalamudTextureWrap Trap { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.Trap).Result;

    public IDalamudTextureWrap BossStatusTimer { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.BossStatusTimer).Result;

    public IDalamudTextureWrap MapNormal { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.MapNormal).Result;

    public IDalamudTextureWrap MapHallOfFallacies { get; } = Service.TextureProvider.CreateFromImageAsync(Resources.MapHallOfFallacies).Result;

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

    private static IFontHandle LoadFont(IUiBuilder uiBuilder, GameFontFamily family, float sizePx)
    {
        return uiBuilder.FontAtlas.NewDelegateFontHandle(x => x.OnPreBuild(toolkit =>
        {
            var fontPtr = toolkit.AddGameGlyphs(new(family, sizePx), null, null);
            toolkit.SetFontScaleMode(fontPtr, FontScaleMode.UndoGlobalScale);
        }));
    }
}