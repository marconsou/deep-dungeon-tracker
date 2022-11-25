using ImGuiNET;
using System.Numerics;

namespace DeepDungeonTracker
{
    public abstract class Button
    {
        protected bool IsMouseOver { get; private set; }

        public Vector2 Position { get; set; }

        private Vector2 Size { get; }

        protected Button(Vector2 size) => this.Size = size;

        public bool OnMouseLeftClick() => this.IsMouseOver && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

        public virtual void Draw(DataUI ui)
        {
            var scale = ui.Scale;
            var mousePos = ImGui.GetMousePos() - ImGui.GetWindowPos();
            var pos = this.Position * scale;
            var size = this.Size * scale;
            this.IsMouseOver =
                mousePos.X >= pos.X && mousePos.X <= pos.X + size.X &&
                mousePos.Y >= pos.Y && mousePos.Y <= pos.Y + size.Y;
        }
    }
}