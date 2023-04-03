using System;
using System.Numerics;

namespace DeepDungeonTracker;

public sealed class DataUI : IDisposable
{
    private bool ShowUI { get; set; }

    private bool IsNowLoadingVisible { get; set; }

    private ResourceUI ResourceUI { get; } = new();

    private Render Render { get; } = new();

    public float Scale { get { return this.Render.Scale; } set { this.Render.Scale = value; } }

    public void Dispose() => this.ResourceUI.Dispose();

    public void Update(bool showAccurateTargetHPPercentage)
    {
        this.ShowUI = Service.ClientState.IsLoggedIn;
        this.IsNowLoadingVisible = NodeUtility.IsNowLoading(Service.GameGui);

        if (Service.ClientState.IsLoggedIn)
        {
            NodeUtility.AccurateTargetHPPercentage(Service.GameGui, Service.TargetManager, "_TargetInfo", 999001888u, 36, showAccurateTargetHPPercentage);
            NodeUtility.AccurateTargetHPPercentage(Service.GameGui, Service.TargetManager, "_TargetInfoMainTarget", 999002888u, 5, showAccurateTargetHPPercentage);
        }
    }

    public bool CommonWindowVisibility(bool show, bool showInBetweenFloors, bool isInDeepDungeonRegion, bool isInsideDeepDungeon)
    {
        return this.ShowUI && show && isInDeepDungeonRegion &&
            (showInBetweenFloors || (!showInBetweenFloors && !this.IsNowLoadingVisible)) &&
            (ServiceUtility.IsSolo || (!ServiceUtility.IsSolo && isInDeepDungeonRegion && !isInsideDeepDungeon));
    }

    public void BuildFonts() => this.ResourceUI.BuildFonts();

    public Vector2 DrawTextAxis(float x, float y, string text, Vector4 color, Alignment align = Alignment.Left, bool calcTextSize = false) => this.Render.DrawText(this.ResourceUI.Axis, x, y, text, color, align, calcTextSize);

    public void DrawTextMiedingerMid(float x, float y, string text, Vector4 color, Alignment align = Alignment.Left) => this.Render.DrawText(this.ResourceUI.MiedingerMid, x, y, text, color, align);

    public void DrawTextMiedingerMidLarge(float x, float y, string text, Vector4 color, Alignment align = Alignment.Left) => this.Render.DrawText(this.ResourceUI.MiedingerMidLarge, x, y, text, color, align);

    public void DrawTextTrumpGothic(float x, float y, string text, Vector4 color, Alignment align = Alignment.Left) => this.Render.DrawText(this.ResourceUI.TrumpGothic, x, y, text, color, align);

    public Vector2 GetAxisTextSize(string text) => Render.GetTextSize(this.ResourceUI.Axis, text);

    public Vector2 GetMiedingerMidLargeTextSize(string text) => Render.GetTextSize(this.ResourceUI.MiedingerMidLarge, text);

    public void DrawNumber(float x, float y, int number, bool isLargeNumber = false, Vector4? color = null, Alignment align = Alignment.Left) => this.Render.DrawNumber(this.ResourceUI.UI, x, y, 1.0f / (!isLargeNumber ? 3.0f : 2.0f), number, color, align);

    public void DrawCheckMark(float x, float y, bool checkMark) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.5f, Convert.ToInt32(checkMark), 2, 1, new(346.0f, 33.0f), new(108.0f, 54.0f), null, Alignment.Center);

    public void DrawDeepDungeon(float x, float y, DeepDungeon deepDungeon, Alignment align = Alignment.Left) => this.Render.DrawUIElement(this.ResourceUI.DeepDungeon, x, y, 0.75f, (int)deepDungeon - 1, 1, 3, align: align);

    public void DrawJob(float x, float y, uint jobId) => this.Render.DrawUIElement(this.ResourceUI.Job, x, y, 0.75f, (int)jobId, 4, 6, align: Alignment.Center);

    public void DrawMiscellaneous(float x, float y, Miscellaneous miscellaneous) => this.Render.DrawUIElement(this.ResourceUI.Miscellaneous, x, y, 0.5f, (int)miscellaneous, 4, 3);

    public void DrawCoffer(float x, float y, Coffer coffer) => this.Render.DrawUIElement(this.ResourceUI.Coffer, x, y, 0.5f, (int)coffer, 10, 7);

    public void DrawEnchantment(float x, float y, Enchantment enchantment, bool isEnchantmentSerenized) => this.Render.DrawUIElement(this.ResourceUI.Enchantment, x, y, 0.5f, (int)enchantment, 4, 4, null, null, !isEnchantmentSerenized ? Color.White : new(1.0f, 1.0f, 1.0f, 0.25f));

    public void DrawTrap(float x, float y, Trap trap) => this.Render.DrawUIElement(this.ResourceUI.Trap, x, y, 0.5f, (int)trap, 3, 3, null, null, Color.Red);

    public void DrawPomander(float x, float y, Pomander pomander) => this.Render.DrawUIElement(this.ResourceUI.Coffer, x, y, 0.5f, (int)pomander, 10, 7);

    public void DrawMapNormal(float x, float y, int id, bool isMapRevealed) => this.Render.DrawUIElement(this.ResourceUI.MapNormal, x, y, 1.0f / 8.0f, id, 4, 4, color: isMapRevealed ? Color.White : Color.Gray);

    public void DrawMapHallOfFallacies(float x, float y, int id, bool isMapRevealed) => this.Render.DrawUIElement(this.ResourceUI.MapHallOfFallacies, x, y, 1.0f / 3.67f, id, 3, 3, color: isMapRevealed ? Color.White : Color.Gray);

    public void DrawMainWindowButton(float x, float y, bool isMouseOver) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.75f, Convert.ToInt32(isMouseOver), 2, 1, new(464.0f, 14.0f), new(72.0f, 36.0f), null, Alignment.Left);

    public void DrawScreenshotButton(float x, float y, bool isMouseOver) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.75f, Convert.ToInt32(isMouseOver), 2, 1, new(156.0f, 61.0f), new(72.0f, 36.0f), null, Alignment.Left);

    public void DrawBackupButton(float x, float y, bool isMouseOver) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.75f, Convert.ToInt32(isMouseOver), 2, 1, new(2.0f, 275.0f), new(64.0f, 32.0f), null, Alignment.Left);

    public void DrawOpenFolderButton(float x, float y, bool isMouseOver) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.75f, Convert.ToInt32(isMouseOver), 2, 1, new(2.0f, 236.0f), new(72.0f, 36.0f), null, Alignment.Left);

    public void DrawArrowButton(float x, float y, bool isMouseOver, bool mirrorHorizontal = false) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.5f, Convert.ToInt32(isMouseOver), 2, 1, new(65.0f, 2.0f), new(144.0f, 56.0f), null, Alignment.Left, mirrorHorizontal);

    public void DrawDoubleArrowButton(float x, float y, bool isMouseOver, bool mirrorHorizontal = false) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.75f, Convert.ToInt32(isMouseOver), 2, 1, new(65.0f, 61.0f), new(88.0f, 34.0f), null, Alignment.Left, mirrorHorizontal);

    public void DrawDeleteButton(float x, float y, bool isMouseOver) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.25f, Convert.ToInt32(isMouseOver), 2, 1, new(214.0f, 236.0f), new(160.0f, 80.0f), null, Alignment.Left);

    public void DrawCloseButton(float x, float y, bool isMouseOver) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.5f, Convert.ToInt32(isMouseOver), 2, 1, new(231.0f, 33.0f), new(112.0f, 56.0f), null, Alignment.Left);

    public void DrawGenericButtonOver(float x, float y, float width, float height) => this.Render.DrawGenericButtonOver(this.ResourceUI.UI, x, y, width, height);

    public void DrawTextButtonOver(float x, float y, float width, float height) => this.Render.DrawTextButtonOver(this.ResourceUI.UI, x, y, width, height);

    public void DrawNumberButtonActive(float x, float y) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.5f, 0, 1, 1, new(464.0f, 53.0f), new(77.0f, 44.0f));

    public void DrawDivisorHorizontal(float x, float y, float width) => this.Render.DrawDivisorHorizontal(this.ResourceUI.UI, x, y, width);

    public void DrawDivisorVertical(float x, float y, float height) => this.Render.DrawDivisorVertical(this.ResourceUI.UI, x, y, height);

    public void DrawBackground(float width, float height, bool isFocused) => this.Render.DrawBackground(this.ResourceUI.UI, width, height, isFocused);

    public void DrawPotsherdRegenPotion(float x, float y, int id) => this.Render.DrawUIElement(this.ResourceUI.UI, x, y, 0.5f, id, 3, 1, new(2.0f, 153.0f), new(240.0f, 80.0f));
}