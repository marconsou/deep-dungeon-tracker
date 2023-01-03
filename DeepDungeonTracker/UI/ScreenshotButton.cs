namespace DeepDungeonTracker
{
    public sealed class ScreenshotButton : Button
    {
        public ScreenshotButton() : base(new(27.0f, 27.0f)) { }

        public override void Draw(DataUI ui, DataAudio audio)
        {
            base.Draw(ui, audio);
            ui.DrawScreenshotButton(this.Position.X, this.Position.Y, this.IsMouseOver);
        }
    }
}