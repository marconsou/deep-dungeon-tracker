namespace DeepDungeonTracker
{
    public sealed class GenericButton : Button
    {
        public GenericButton(float width = 0.0f, float height = 0.0f) : base(new(width, height)) { }

        public override void Draw(DataUI ui, DataAudio audio)
        {
            base.Draw(ui, audio);
            if (this.IsMouseOver)
                ui.DrawGenericButtonOver(this.Position.X - 10.0f, this.Position.Y, this.Size.X, this.Size.Y + 5.0f);
        }
    }
}