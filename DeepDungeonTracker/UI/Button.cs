using ImGuiNET;
using System.Numerics;

namespace DeepDungeonTracker;

public abstract class Button
{
    protected bool IsMouseOver { get; private set; }

    private bool PreviousIsMouseOver { get; set; }

    public bool Show { get; set; } = true;

    public Vector2 Position { get; set; }

    public Vector2 Size { get; set; }

    protected Button(Vector2 size) => this.Size = size;

    public bool OnMouseLeftClick() => this.Show && this.IsMouseOver && ImGui.IsMouseClicked(ImGuiMouseButton.Left);

    public virtual void Draw(DataUI ui, DataAudio audio)
    {
        if (!this.Show)
        {
            this.IsMouseOver = false;
            this.PreviousIsMouseOver = false;
            return;
        }

        var scale = ui?.Scale ?? 1.0f;
        var mousePos = ImGui.GetMousePos() - ImGui.GetWindowPos();
        var pos = this.Position * scale;
        var size = this.Size * scale;
        this.IsMouseOver =
            mousePos.X >= pos.X && mousePos.X <= pos.X + size.X &&
            mousePos.Y >= pos.Y && mousePos.Y <= pos.Y + size.Y;

        if (!this.PreviousIsMouseOver && this.IsMouseOver)
            audio?.PlaySound(SoundIndex.OnMouseOver);

        this.PreviousIsMouseOver = this.IsMouseOver;
    }
}