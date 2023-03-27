namespace DeepDungeonTracker;

public sealed class ArrowButton : Button
{
    private bool MirrorHorizontal { get; }

    public ArrowButton(bool mirrorHorizontal) : base(new(36.0f, 28.0f)) => this.MirrorHorizontal = mirrorHorizontal;

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawArrowButton(this.Position.X, this.Position.Y, this.IsMouseOver, this.MirrorHorizontal);
    }
}