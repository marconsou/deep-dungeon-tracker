namespace DeepDungeonTracker;

public sealed class DoubleArrowButton : Button
{
    private bool MirrorHorizontal { get; }

    public DoubleArrowButton(bool mirrorHorizontal) : base(new(33.0f, 25.5f)) => this.MirrorHorizontal = mirrorHorizontal;

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawDoubleArrowButton(this.Position.X, this.Position.Y, this.IsMouseOver, this.MirrorHorizontal);
    }
}