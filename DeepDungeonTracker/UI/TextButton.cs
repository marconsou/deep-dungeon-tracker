namespace DeepDungeonTracker;

public sealed class TextButton : Button
{
    public TextButton(float width = 0.0f, float height = 0.0f) : base(new(width, height)) { }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        if (this.IsMouseOver)
        {
            var padX = 15.0f;
            var padY = 5.0f;
            ui.DrawTextButtonOver(this.Position.X - padX, this.Position.Y - padY, this.Size.X + (padX * 2.0f), this.Size.Y + (padY * 2.0f));
        }
    }
}