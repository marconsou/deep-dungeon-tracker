using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DeepDungeonTracker;

public abstract class WindowEx : Window
{
    protected static ImGuiWindowFlags Static => ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize;

    protected static ImGuiWindowFlags StaticNoBackground => WindowEx.Static | ImGuiWindowFlags.NoBackground;

    protected static ImGuiWindowFlags StaticNoBackgroundMove => WindowEx.StaticNoBackground ^ ImGuiWindowFlags.NoMove;

    protected static ImGuiWindowFlags StaticNoBackgroundMoveInputs => WindowEx.StaticNoBackgroundMove ^ ImGuiWindowFlags.NoInputs;

    private Vector2 Padding { get; set; }

    private float Rounding { get; set; }

    protected Configuration Configuration { get; }

    public static string GetWindowId(string id, string className) => $"{className?.Replace("Window", string.Empty, StringComparison.InvariantCultureIgnoreCase)}##{id}";

    public static void DisposeWindows(IReadOnlyList<Window> windows)
    {
        if (windows == null)
            return;

        foreach (var item in windows)
            (item as IDisposable)?.Dispose();
    }

    protected WindowEx(string id, Configuration configuration, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(string.Empty, flags, forceMainWindow)
    {
        this.Configuration = configuration;
        this.WindowName = WindowEx.GetWindowId(id, this.GetType().Name);
    }

    public override void PreDraw()
    {
        if ((this.Flags & ImGuiWindowFlags.NoTitleBar) != 0)
        {
            var style = ImGui.GetStyle();

            this.Padding = new Vector2(style.WindowPadding.X, style.WindowPadding.Y);
            this.Rounding = new Vector2(style.WindowRounding).X;

            style.WindowPadding = Vector2.Zero;
            style.WindowRounding = 0.0f;
        }
    }

    public override void PostDraw()
    {
        if ((this.Flags & ImGuiWindowFlags.NoTitleBar) != 0)
        {
            var style = ImGui.GetStyle();

            style.WindowPadding = this.Padding;
            style.WindowRounding = this.Rounding;
        }
    }

    protected Vector2 GetSizeScaled()
    {
        var fontGlobalScale = ImGui.GetIO().FontGlobalScale;
        return this.Size * (fontGlobalScale >= 1.0f ? 1.0f : fontGlobalScale) ?? Vector2.One;
    }

    protected void WindowSizeUpdate(float width, float height, float scale)
    {
        var fontGlobalScale = ImGui.GetIO().FontGlobalScale;
        this.Size = new Vector2(width * scale, height * scale) / (fontGlobalScale >= 1.0f ? 1.0f : fontGlobalScale);
    }

    protected static void Tooltip(string message, float width = 420.0f)
    {
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(width);
            ImGui.Text(message);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        }
    }

    protected static void Disabled(Action action, bool disabled, float disabledAlpha = 0.2f)
    {
        var style = ImGui.GetStyle();
        var currentDisabledAlpha = style.DisabledAlpha;

        style.DisabledAlpha = disabledAlpha;

        ImGui.BeginDisabled(disabled);
        action?.Invoke();
        ImGui.EndDisabled();

        style.DisabledAlpha = currentDisabledAlpha;
    }

    protected static bool Child(Action action, string id, float width, float height)
    {
        var result = ImGui.BeginChild(id, new Vector2(width, height));
        action?.Invoke();
        ImGui.EndChild();
        return result;
    }

    private (bool, T) Control<T>(Func<T, (bool, T)> function, T value, Action<T> setter, bool save = true)
    {
        var result = function(value);
        if (result.Item1)
        {
            setter(result.Item2);
            if (save)
                this.Configuration.Save();
        }
        return result;
    }

    protected bool Button(Action action, string label, bool save = false)
    {
        return this.Control((param) =>
        {
            return (ImGui.Button(label), param);
        }, false, (setterParam) => action(), save).Item1;
    }

    protected bool SmallButton(Action action, string label, bool save = false)
    {
        return this.Control((param) =>
        {
            return (ImGui.SmallButton(label), param);
        }, false, (setterParam) => action(), save).Item1;
    }

    protected bool ArrowButton(Action action, string label, ImGuiDir direction, bool save = false)
    {
        return this.Control((param) =>
        {
            return (ImGui.ArrowButton(label, direction), param);
        }, false, (setterParam) => action(), save).Item1;
    }

    protected bool IconButton(Action action, FontAwesomeIcon icon, string id, bool save = false)
    {
        return this.Control((param) =>
        {
            return (ImGuiComponents.IconButton($"{icon.ToIconString()}##{id}"), param);
        }, false, (setterParam) => action(), save).Item1;
    }

    protected (bool, int) DragInt(int value, Action<int> setter, string label, int speed, int min, int max, string format)
    {
        if (setter == null)
            return new();

        return this.Control((param) =>
        {
            return (ImGui.DragInt(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param);
        }, value, setter);
    }

    protected (bool, int, bool, int) DragInt(int valueLeft, int valueRight, Action<int> setterLeft, Action<int> setterRight, string label, int speed, int min, int max, string format)
    {
        if (setterLeft == null || setterRight == null)
            return new();

        var result1 = this.Control((param) =>
        {
            return (ImGui.DragInt($"##{label}", ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param);
        }, valueLeft, setterLeft);

        var result2 = this.Control((param) =>
        {
            return (ImGui.DragInt(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param);
        }, valueRight, setterRight);
        return (result1.Item1, result1.Item2, result2.Item1, result2.Item2);
    }

    protected (bool, float) DragFloat(float value, Action<float> setter, string label, float speed, float min, float max, string format)
    {
        if (setter == null)
            return new();

        return this.Control((param) =>
        {
            return (ImGui.DragFloat(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param);
        }, value, setter);
    }

    protected (bool, float, bool, float) DragFloat(float valueLeft, float valueRight, Action<float> setterLeft, Action<float> setterRight, string label, float speed, float min, float max, string format)
    {
        if (setterLeft == null || setterRight == null)
            return new();

        var result1 = this.Control((param) =>
        {
            return (ImGui.DragFloat($"##{label}", ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param);
        }, valueLeft, setterLeft);

        var result2 = this.Control((param) =>
        {
            return (ImGui.DragFloat(label, ref param, speed, min, max, format, ImGuiSliderFlags.AlwaysClamp), param);
        }, valueRight, setterRight);
        return (result1.Item1, result1.Item2, result2.Item1, result2.Item2);
    }

    protected (bool, Vector4) ColorEdit4(Vector4 value, Action<Vector4> setter, string label)
    {
        if (setter == null)
            return new();

        return this.Control((param) =>
        {
            return (ImGui.ColorEdit4(label, ref param, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.AlphaBar), param);
        }, value, setter);
    }

    protected (bool, bool) CheckBox(bool value, Action<bool> setter, string label)
    {
        if (setter == null)
            return new();

        return this.Control((param) =>
        {
            return (ImGui.Checkbox(label, ref param), param);
        }, value, setter);
    }

    protected (bool, T) Combo<T>(T value, Action<T> setter, string label) where T : Enum
    {
        if (setter == null)
            return new();

        var names = value.GetNames();
        return this.Control((param) =>
        {
            var convertedParam = (int)(object)param;
            return (ImGui.Combo(label, ref convertedParam, names, names.Length), (T)(object)convertedParam);
        },
        value, setter);
    }
}