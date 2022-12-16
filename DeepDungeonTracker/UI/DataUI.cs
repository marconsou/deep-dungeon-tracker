using System;
using System.Numerics;

namespace DeepDungeonTracker
{
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

        public void DrawTextMiedingerMediumW00(float x, float y, string text, Vector4 color, Alignment align = Alignment.Left, bool drawShadow = false) => this.Render.DrawText(this.ResourceUI.MiedingerMediumW00, x, y, text.ToUpper(), color, align, drawShadow);

        public void DrawTextAxisLatinPro(float x, float y, string text, Vector4 color, Alignment align = Alignment.Left, bool drawShadow = false) => this.Render.DrawText(this.ResourceUI.AxisLatinPro, x, y, text, color, align, drawShadow);

        public Vector2 GetMiedingerMediumW00TextSize(string text) => Render.GetTextSize(this.ResourceUI.MiedingerMediumW00.FontLayout, text);

        public Vector2 GetAxisLatinProTextSize(string text) => Render.GetTextSize(this.ResourceUI.AxisLatinPro.FontLayout, text);

        public void DrawNumber(float x, float y, int number, bool isLargeNumber = false, Vector4? color = null, Alignment align = Alignment.Left) => this.Render.DrawNumber(this.ResourceUI.Number, x, y, 1.0f / (!isLargeNumber ? 3.0f : 2.0f), number, color, align);

        public void DrawCheckMark(float x, float y, bool checkMark) => this.Render.DrawUIElement(this.ResourceUI.CheckMark, x, y, 0.5f, Convert.ToInt32(checkMark), 2, 1, null, Alignment.Center);

        public void DrawMiscellaneous(float x, float y, Miscellaneous miscellaneous) => this.Render.DrawUIElement(this.ResourceUI.Miscellaneous, x, y, 0.5f, (int)miscellaneous, 4, 3);

        public void DrawCoffer(float x, float y, Coffer coffer) => this.Render.DrawUIElement(this.ResourceUI.Coffer, x, y, 0.5f, (int)coffer, 6, 5);

        public void DrawEnchantment(float x, float y, Enchantment enchantment) => this.Render.DrawUIElement(this.ResourceUI.Enchantment, x, y, 0.5f, (int)enchantment, 4, 4);

        public void DrawTrap(float x, float y, Trap trap) => this.Render.DrawUIElement(this.ResourceUI.Trap, x, y, 0.5f, (int)trap, 3, 2, Color.Red);

        public void DrawPomander(float x, float y, Pomander pomander) => this.Render.DrawUIElement(this.ResourceUI.Coffer, x, y, 0.5f, (int)pomander, 6, 5);

        public void DrawMapNormal(float x, float y, int id) => this.Render.DrawUIElement(this.ResourceUI.MapNormal, x, y, 1.0f / 9.0f, id, 4, 4);

        public void DrawMapHallOfFallacies(float x, float y, int id) => this.Render.DrawUIElement(this.ResourceUI.MapHallOfFallacies, x, y, 1.0f / 9.0f, id, 3, 3);

        public void DrawArrowButton(float x, float y, bool isMouseOver, bool mirrorHorizontal = false) => this.Render.DrawUIElement(this.ResourceUI.ArrowButton, x, y, 0.5f, Convert.ToInt32(isMouseOver), 2, 1, null, Alignment.Left, mirrorHorizontal);

        public void DrawCloseButton(float x, float y, bool isMouseOver) => this.Render.DrawUIElement(this.ResourceUI.CloseButton, x, y, 0.5f, Convert.ToInt32(isMouseOver), 2, 1);

        public void DrawDivisorHorizontal(float x, float y, float width) => this.Render.DrawDivisorHorizontal(this.ResourceUI.DivisorHorizontal, x, y, width);

        public void DrawDivisorVertical(float x, float y, float height) => this.Render.DrawDivisorVertical(this.ResourceUI.DivisorVertical, x, y, height);

        public void DrawBackground(float width, float height, bool isFocused) => this.Render.DrawBackground(this.ResourceUI.Background, width, height, isFocused);
    }
}