using DeepDungeonTracker.Properties;
using ImGuiScene;
using System;

namespace DeepDungeonTracker
{
    public sealed class ResourceUI : IDisposable
    {
        public Font MiedingerMediumW00 { get; }

        public Font AxisLatinPro { get; }

        public TextureWrap Number { get; }

        public TextureWrap CheckMark { get; }

        public TextureWrap Miscellaneous { get; }

        public TextureWrap Coffer { get; }

        public TextureWrap Enchantment { get; }

        public TextureWrap Trap { get; }

        public TextureWrap MapNormal { get; }

        public TextureWrap MapHallOfFallacies { get; }

        public TextureWrap ArrowButton { get; }

        public TextureWrap CloseButton { get; }

        public TextureWrap DivisorHorizontal { get; }

        public TextureWrap DivisorVertical { get; }

        public TextureWrap Background { get; }

        public ResourceUI()
        {
            this.MiedingerMediumW00 = new(Resources.MiedingerMediumW00Layout, Resources.MiedingerMediumW00Font);
            this.AxisLatinPro = new(Resources.AxisLatinProLayout, Resources.AxisLatinProFont);
            this.Number = Service.PluginInterface.UiBuilder.LoadImage(Resources.Number);
            this.CheckMark = Service.PluginInterface.UiBuilder.LoadImage(Resources.CheckMark);
            this.Miscellaneous = Service.PluginInterface.UiBuilder.LoadImage(Resources.Miscellaneous);
            this.Coffer = Service.PluginInterface.UiBuilder.LoadImage(Resources.Coffer);
            this.Enchantment = Service.PluginInterface.UiBuilder.LoadImage(Resources.Enchantment);
            this.Trap = Service.PluginInterface.UiBuilder.LoadImage(Resources.Trap);
            this.MapNormal = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapNormal);
            this.MapHallOfFallacies = Service.PluginInterface.UiBuilder.LoadImage(Resources.MapHallOfFallacies);
            this.ArrowButton = Service.PluginInterface.UiBuilder.LoadImage(Resources.ArrowButton);
            this.CloseButton = Service.PluginInterface.UiBuilder.LoadImage(Resources.CloseButton);
            this.DivisorHorizontal = Service.PluginInterface.UiBuilder.LoadImage(Resources.DivisorHorizontal);
            this.DivisorVertical = Service.PluginInterface.UiBuilder.LoadImage(Resources.DivisorVertical);
            this.Background = Service.PluginInterface.UiBuilder.LoadImage(Resources.Background);
        }

        public void Dispose()
        {
            this.MiedingerMediumW00.Dispose();
            this.AxisLatinPro.Dispose();
            this.Number.Dispose();
            this.CheckMark.Dispose();
            this.Miscellaneous.Dispose();
            this.Coffer.Dispose();
            this.Enchantment.Dispose();
            this.Trap.Dispose();
            this.MapNormal.Dispose();
            this.MapHallOfFallacies.Dispose();
            this.ArrowButton.Dispose();
            this.CloseButton.Dispose();
            this.DivisorHorizontal.Dispose();
            this.DivisorVertical.Dispose();
            this.Background.Dispose();
        }
    }
}