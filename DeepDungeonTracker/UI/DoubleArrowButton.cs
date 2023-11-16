namespace DeepDungeonTracker;

public sealed class DoubleArrowButton(bool mirrorHorizontal) : Button(new(33.0f, 25.5f))
{
    private bool MirrorHorizontal { get; } = mirrorHorizontal;

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawDoubleArrowButton(this.Position.X, this.Position.Y, this.IsMouseOver, this.MirrorHorizontal);
    }
}