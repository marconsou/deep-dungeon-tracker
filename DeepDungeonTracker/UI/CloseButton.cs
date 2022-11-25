namespace DeepDungeonTracker
{
    public sealed class CloseButton : Button
    {
        public CloseButton() : base(new(28.0f, 28.0f)) { }

        public override void Draw(DataUI ui)
        {
            base.Draw(ui);
            ui.DrawCloseButton(this.Position.X, this.Position.Y, this.IsMouseOver);
        }
    }
}