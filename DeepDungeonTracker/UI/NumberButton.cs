using System.Numerics;

namespace DeepDungeonTracker;

public sealed class NumberButton(int number, float width = 0.0f, float height = 0.0f) : Button(new(width, height))
{
    public int Number { get; set; } = number;

    public Vector4 Color { get; set; }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        var text = $"{this.Number}";
        this.Size = ui.GetMiedingerMidLargeTextSize("00");
        var textSize = ui.GetMiedingerMidLargeTextSize(text);
        var centerX = this.Position.X + (this.Size.X / 2.0f) - (textSize.X / 2.0f);
        var centerY = this.Position.Y + (this.Size.Y / 2.0f) - (textSize.Y / 2.0f);
        ui.DrawTextMiedingerMidLarge(centerX, centerY, text, this.Color, Alignment.Left);
    }
}