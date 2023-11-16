namespace DeepDungeonTracker;

public sealed class ArrowButton(bool mirrorHorizontal) : Button(new(36.0f, 28.0f))
{
    private bool MirrorHorizontal { get; } = mirrorHorizontal;

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawArrowButton(this.Position.X, this.Position.Y, this.IsMouseOver, this.MirrorHorizontal);
    }
}