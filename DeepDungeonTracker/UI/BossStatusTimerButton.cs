namespace DeepDungeonTracker;

public sealed class BossStatusTimerButton : Button
{
    public BossStatusTimerButton() : base(new(27.0f, 27.0f)) { }

    public override void Draw(DataUI ui, DataAudio audio)
    {
        base.Draw(ui, audio);
        ui.DrawBossStatusTimerWindowButton(this.Position.X, this.Position.Y, this.IsMouseOver);
    }
}