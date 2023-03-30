namespace DeepDungeonTracker;

public sealed class ScreenshotFolderButton : Button
{
    public ScreenshotFolderButton() : base(new(27.0f, 27.0f)) { }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawScreenshotFolderButton(this.Position.X, this.Position.Y, this.IsMouseOver);
    }
}