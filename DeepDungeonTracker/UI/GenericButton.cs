namespace DeepDungeonTracker
{
    public sealed class GenericButton : Button
    {
        public GenericButton(float width = 0.0f, float height = 0.0f) : base(new(width, height)) { }

        public override void Draw(DataUI ui, DataAudio audio)
        {
            base.Draw(ui, audio);
            if (this.IsMouseOver)
            {
                var padding = 5.0f;
                ui.DrawGenericButtonOver(this.Position.X - padding, this.Position.Y - padding, this.Size.X + padding, this.Size.Y + padding);
            }
        }
    }
}