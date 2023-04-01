namespace DeepDungeonTracker;

public sealed class MainWindowButton : Button
{
    public MainWindowButton() : base(new(27.0f, 27.0f)) { }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawMainWindowButton(this.Position.X, this.Position.Y, this.IsMouseOver);
    }
}