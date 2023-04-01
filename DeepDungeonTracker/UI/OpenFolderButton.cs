namespace DeepDungeonTracker;

public sealed class OpenFolderButton : Button
{
    public OpenFolderButton() : base(new(27.0f, 27.0f)) { }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawOpenFolderButton(this.Position.X, this.Position.Y, this.IsMouseOver);
    }
}