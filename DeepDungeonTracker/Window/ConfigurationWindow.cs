using ImGuiNET;
using System;
using System.Text.Json;

namespace DeepDungeonTracker;

public sealed class ConfigurationWindow : WindowEx, IDisposable
{
    private Data Data { get; }

    private string[] FieldNames { get; }

    public ConfigurationWindow(string id, Configuration configuration, Data data) : base(id, configuration, ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.Data = data;
        this.FieldNames = new string[] { "Kills", "Mimics", "Mandragoras", "Mimicgoras", "NPCs/Dread Beasts", "Coffers", "Enchantments", "Traps", "Deaths", "Regen Potions", "Potsherds", "Lurings", "Maps", "Time Bonuses" };
        this.SizeConstraints = new() { MaximumSize = new(600.0f, 600.0f) };
    }

    public void Dispose() { }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("Tab Bar"))
        {
            if (ImGui.BeginTabItem("General"))
            {
                this.General();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Tracker"))
            {
                this.Tracker();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Floor Set Time"))
            {
                this.FloorSetTime();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Score"))
            {
                this.Score();
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem("Statistics"))
            {
                this.Statistics();
                ImGui.EndTabItem();
            }
            ImGui.EndTabBar();
        }
        ImGui.Separator();
        this.Button(this.Configuration.Reset, "Reset all settings to default", true);
    }

    private void General()
    {
        var config = this.Configuration.General;
        this.CheckBox(config.ShowAccurateTargetHPPercentage, x => config.ShowAccurateTargetHPPercentage = x, "Show accurate target HP %");
        WindowEx.Tooltip("It doesn't apply to Focus Target.");

        if (ImGui.CollapsingHeader("Main Window"))
        {
            this.CheckBox(config.MainWindowSolidBackground, x => config.MainWindowSolidBackground = x, "Solid Background");
            this.DragFloat(config.MainWindowScale, x => config.MainWindowScale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
        }

        if (ImGui.CollapsingHeader("Information"))
        {
            ImGui.TextColored(Color.Green, "Kills:");
            ImGui.TextWrapped(
                "All enemies killed from a distance of more than two rooms cannot be counted." +
                "\nIf you use a magicite, do so in the center of the floor, covering all enemies killed (as much as possible).");
            ImGui.TextColored(Color.Green, "Cairn of Passage Kills:");
            ImGui.TextWrapped("Keep your map menu open to verify the Cairn of Passage key status. The value can be inaccurate if you kill too many enemies at the same time.");
            ImGui.TextColored(Color.Green, "Maps:");
            ImGui.TextWrapped("Keep your map menu open to verify the map reveal.");
            ImGui.TextColored(Color.Green, "Potsherds:");
            ImGui.TextWrapped("Only Potsherds obtained from bronze coffers will be counted.");
            ImGui.TextColored(Color.Green, "Score:");
            ImGui.TextWrapped("The number shown in the Score Window is the Duty Complete value.\nThe score will be zero if you start tracking it from an ongoing save file.");
            ImGui.TextColored(Color.Green, "Save Files:");
            ImGui.TextWrapped(
                "Save files are automatically created once you enter a Deep Dungeon. They are deleted once you delete an in-game Save Slot and reopen the in-game Save Slot menu. These files cannot be renamed." +
                "\nYou can backup your saved files, open the Backup folder, and rename them as you want.");
        }

        if (ImGui.CollapsingHeader("OpCodes"))
        {
            ImGui.TextWrapped("Values in this section should not be zero.");
            ImGui.TextWrapped($"{JsonSerializer.Serialize(this.Configuration.OpCodes, new JsonSerializerOptions() { WriteIndented = true, })}");
        }
    }

    private void Tracker()
    {
        var config = this.Configuration.Tracker;
        this.CheckBox(config.Lock, x => config.Lock = x, "Lock");
        ImGui.SameLine();
        this.CheckBox(config.SolidBackground, x => config.SolidBackground = x, "Solid Background");
        this.CheckBox(config.Show, x => config.Show = x, "Show");
        WindowEx.Tooltip("You need to be either inside a Deep Dungeon or in the area outside to get into it.");
        ImGui.SameLine();
        this.CheckBox(config.ShowInBetweenFloors, x => config.ShowInBetweenFloors = x, "Show in between floors");
        this.CheckBox(config.ShowFloorEffectPomanders, x => config.ShowFloorEffectPomanders = x, "Show floor effect pomanders");
        WindowEx.Tooltip("Show pomander icons (at the top left of the window) representing their effect on the current floor.");
        this.Combo(config.FontType, x => config.FontType = x, "Font");
        this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
        this.CheckBox(config.IsFloorNumberVisible, x => config.IsFloorNumberVisible = x, "##IsFloorNumberVisible");
        ImGui.SameLine();
        this.ColorEdit4(config.FloorNumberColor, x => config.FloorNumberColor = x, "Floor");
        ImGui.SameLine();
        this.CheckBox(config.IsSetNumberVisible, x => config.IsSetNumberVisible = x, "##IsSetNumberVisible");
        ImGui.SameLine();
        this.ColorEdit4(config.SetNumberColor, x => config.SetNumberColor = x, "Set");
        ImGui.SameLine();
        this.CheckBox(config.IsTotalNumberVisible, x => config.IsTotalNumberVisible = x, "##IsTotalNumberVisible");
        ImGui.SameLine();
        this.ColorEdit4(config.TotalNumberColor, x => config.TotalNumberColor = x, "Total");

        var left = ImGui.GetCursorPosX();
        for (var i = 0; i < config.Fields?.Length; i++)
        {
            var field = config.Fields[i];
            var fieldName = this.FieldNames[field.Index];
            var lastIndex = config.Fields.Length - 1;

            this.ArrowButton(() =>
            {
                var source = i;
                var target = i > 0 ? i - 1 : lastIndex;
                (config.Fields[source], config.Fields[target]) = (config.Fields[target], config.Fields[source]);
            }, $"##Up{fieldName}", ImGuiDir.Up, true);
            ImGui.SameLine();

            this.ArrowButton(() =>
            {
                var source = i;
                var target = i < lastIndex ? i + 1 : 0;
                (config.Fields[source], config.Fields[target]) = (config.Fields[target], config.Fields[source]);
            }, $"##Down{fieldName}", ImGuiDir.Down, true);
            ImGui.SameLine();

            this.CheckBox(field.Show, x => field.Show = x, $"#{i + 1:D2} {fieldName}");
        }
    }

    private void FloorSetTime()
    {
        var config = this.Configuration.FloorSetTime;
        this.CheckBox(config.Lock, x => config.Lock = x, "Lock");
        ImGui.SameLine();
        this.CheckBox(config.SolidBackground, x => config.SolidBackground = x, "Solid Background");
        this.CheckBox(config.Show, x => config.Show = x, "Show");
        WindowEx.Tooltip("You need to be either inside a Deep Dungeon or in the area outside to get into it.");
        ImGui.SameLine();
        this.CheckBox(config.ShowInBetweenFloors, x => config.ShowInBetweenFloors = x, "Show in between floors");
        ImGui.SameLine();
        this.CheckBox(config.ShowTitle, x => config.ShowTitle = x, "Show title");
        this.CheckBox(config.ShowFloorTime, x => config.ShowFloorTime = x, "Show floor time");
        this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
        this.ColorEdit4(config.PreviousFloorTimeColor, x => config.PreviousFloorTimeColor = x, "Previous Floor Time");
        ImGui.SameLine();
        this.ColorEdit4(config.CurrentFloorTimeColor, x => config.CurrentFloorTimeColor = x, "Current Floor Time");
        this.ColorEdit4(config.AverageTimeColor, x => config.AverageTimeColor = x, "Average Time");
        ImGui.SameLine();
        this.ColorEdit4(config.RespawnTimeColor, x => config.RespawnTimeColor = x, "Respawn Time");
    }

    private void Score()
    {
        var config = this.Configuration.Score;
        this.CheckBox(config.Lock, x => config.Lock = x, "Lock");
        ImGui.SameLine();
        this.CheckBox(config.SolidBackground, x => config.SolidBackground = x, "Solid Background");
        this.CheckBox(config.Show, x => config.Show = x, "Show");
        WindowEx.Tooltip("You need to be either inside a Deep Dungeon or in the area outside to get into it.");
        ImGui.SameLine();
        this.CheckBox(config.ShowInBetweenFloors, x => config.ShowInBetweenFloors = x, "Show in between floors");
        ImGui.SameLine();
        this.CheckBox(config.ShowTitle, x => config.ShowTitle = x, "Show title");
        this.Combo(config.FontType, x => config.FontType = x, "Font");
        this.Combo(config.ScoreCalculationType, x => config.ScoreCalculationType = x, "Score Calculation");
        WindowEx.Tooltip(
            "Current Floor: Include all floor completion-related score up to the current floor and current character level.\nYou can see your score progressively increasing each time you go to the next floor or level up.\n\n" +
            "Score Window Floor: Include all floor completion-related score up to the floor where it shows the next score window.\n\n" +
            "Last Floor: Include all floor completion-related score at once.\n\n"+
            "Floor completion-related score has nothing to do with map reveals, and this affects the points earned by killing enemies. It's recommended to change this option before starting a fresh save file.");
        this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
        this.CheckBox(config.IsFlyTextScoreVisible, x => config.IsFlyTextScoreVisible = x, "##IsFlyTextScoreVisible");
        WindowEx.Tooltip("When the score changes, a Fly Text will be shown.");
        ImGui.SameLine();
        this.ColorEdit4(config.FlyTextScoreColor, x => config.FlyTextScoreColor = x, "Fly Text Score");
        ImGui.SameLine();
        this.ColorEdit4(config.TotalScoreColor, x => config.TotalScoreColor = x, "Total Score");
    }

    private void Statistics()
    {
        var config = this.Configuration.Statistics;
        var statistics = this.Data.Statistics;
        this.CheckBox(config.SolidBackground, x => config.SolidBackground = x, "Solid Background");
        this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
        this.ColorEdit4(config.FloorTimeColor, x => config.FloorTimeColor = x, "Floor Time");
        ImGui.SameLine();
        this.ColorEdit4(config.ScoreColor, x => config.ScoreColor = x, "Score");
        ImGui.SameLine();
        this.ColorEdit4(config.SummarySelectionColor, x => config.SummarySelectionColor = x, "Summary Selection");
    }
}