namespace DeepDungeonTracker;

public sealed class DeleteButton : Button
{
    public DeleteButton() : base(new(20.0f, 20.0f)) { }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawDeleteButton(this.Position.X, this.Position.Y, this.IsMouseOver);
    }
}