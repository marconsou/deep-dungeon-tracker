namespace DeepDungeonTracker;

public sealed class BackupButton : Button
{
    public BackupButton() : base(new(27.0f, 27.0f)) { }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawBackupButton(this.Position.X, this.Position.Y, this.IsMouseOver);
    }
}