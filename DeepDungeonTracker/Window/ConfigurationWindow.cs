using ImGuiNET;
using System;
using System.Text.Json;

namespace DeepDungeonTracker;

public sealed class ConfigurationWindow : WindowEx, IDisposable
{
    private Action MainWindowToggleVisibility { get; }

    private string[] FieldNames { get; }

    public ConfigurationWindow(string id, Configuration configuration, Action mainWindowToggleVisibility) : base(id, configuration, ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.MainWindowToggleVisibility = mainWindowToggleVisibility;
        this.FieldNames = new string[] { "Kills", "Mimics", "Mandragoras", "Mimicgoras", "NPCs/Dread Beasts", "Coffers", "Enchantments", "Traps", "Deaths", "Regen Potions", "Potsherds/Fragments", "Lurings", "Maps", "Time Bonuses" };
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
            if (ImGui.BeginTabItem("Main"))
            {
                this.Main();
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
            if (ImGui.BeginTabItem("Boss Status Timer"))
            {
                this.BossStatusTimer();
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
        ImGui.Text($"Click to open the");
        ImGui.SameLine();
        this.SmallButton(() => this.MainWindowToggleVisibility(), "Main Window");
        ImGui.SameLine();
        ImGui.Text($"or type {Commands.MainCommand}");
        this.CheckBox(config.ShowAccurateTargetHPPercentage, x => config.ShowAccurateTargetHPPercentage = x, "Show accurate target HP %");
        WindowEx.Tooltip("It doesn't apply to Focus Target.");

        if (ImGui.CollapsingHeader("Information"))
        {
            ImGui.TextColored(Color.Green, "Kills:");
            ImGui.TextWrapped(
                "All enemies killed from a distance of more than two rooms (any situation) cannot be counted." +
                "\nIf you use a Magicite, do so in the center of the floor, covering all enemies killed (as much as possible).");
            ImGui.TextColored(Color.Green, "Kills to open the Passage:");
            ImGui.TextWrapped(
                "Keep your map menu open to verify the Passage Key status." +
                "\nThe value can be inaccurate if you kill too many enemies at the same time.");
            ImGui.TextColored(Color.Green, "Maps:");
            ImGui.TextWrapped("Keep your map menu open to verify the map reveal.");
            ImGui.TextColored(Color.Green, "Potsherds/Fragments:");
            ImGui.TextWrapped("Only obtained from bronze coffers will be counted.");
            ImGui.TextColored(Color.Green, "Score:");
            ImGui.TextWrapped("The number shown in the Score Window is the Duty Complete value.\nThe score will be zero if you start tracking it from an ongoing save file.");
            ImGui.TextColored(Color.Green, "Save Files:");
            ImGui.TextWrapped(
                "Save files are automatically created once you enter a Deep Dungeon." +
                "\nWhen you delete an in-game save slot and then reopen the in-game Save Slot Menu, the saved file associated to that slot will be moved to the Last Save option, instead of being deleted. These files cannot be renamed." +
                "\nYou can backup your saved files, open the Backup folder, and rename them as you want.");
            ImGui.TextColored(Color.Green, "Boss Status Timer:");
            ImGui.TextWrapped(
                "The timers shown on this menu can be inaccurate up to one second (due to rounding values). Only durations longer than ten seconds will be shown." +
                "\nThe pomanders shown on this menu are based on the pomanders used during the entire time on the floor, not only the boss fight.");
        }

        if (ImGui.CollapsingHeader("OpCodes"))
            ImGui.TextWrapped($"{JsonSerializer.Serialize(this.Configuration.OpCodes, new JsonSerializerOptions() { WriteIndented = true, })}");

        if (ImGui.CollapsingHeader("Testing"))
        {
            this.CheckBox(config.ImprovedMagiciteKillsDetection, x => config.ImprovedMagiciteKillsDetection = x, "Improved Magicite Kills Detection");

            ImGui.TextWrapped("In Heaven-on-High, when you use a Magicite on a floor, all enemies far from you cannot be counted as killed, and the best you can do to cover as many kills as you can is to use the Magicite at the very center of the floor (hard to execute).");
            ImGui.TextWrapped("Enabling this option will allow for a more accurate kill count, if not 100%% correct.");
            ImGui.TextColored(Color.Green, "How does it work?");
            ImGui.TextWrapped("Every time you get close to the enemies, their data will be stored.");
            ImGui.TextWrapped("When you use a Magicite, instead of counting all enemies close to you during the moment you use it, the counting will be based on the stored data previously mentioned (which includes enemies close to you and potentially enemies far from you if you got close to them at some point before using the Magicite).");
            ImGui.TextColored(Color.Green, "How close do you need to get to an enemy?");
            ImGui.TextWrapped("The distance is approximately two rooms (straight).");
            ImGui.TextWrapped("If you need a visual indication on an open floor (like Big Floors/Hall of Fallacies), when you get close to the enemies, you will see them spawning on your screen with a dark-purple aura around them.");
            ImGui.TextWrapped("This is the required distance, and from this point on, those enemies will be counted when you use a Magicite no matter where you use it (close to or far from them).");
            ImGui.TextWrapped("So this can give you more flexibility in how you use the Magicite. Basically, every enemy you see on your screen for the first time will be counted, even if you get far away from them when using a Magicite.");
            ImGui.TextColored(Color.Green, "Limitations:");
            ImGui.TextWrapped("Using a Magicite as soon as you enter on a floor will not make it possible to count all the kills (unless you start close to all enemies at once).");
            ImGui.TextWrapped("A respawn far away from you will not be possible to count as a kill (unless you go back and get close to them; distance is mentioned above).");
            ImGui.TextWrapped("The limitations mentioned above are already present even if you disable this option.");
            ImGui.TextWrapped("So, the more you move on a floor around the rooms before you decide to use a Magicite, more accurate it can be, potentially covering all enemies.");
        }
    }

    private void Main()
    {
        var config = this.Configuration.Main;
        this.CheckBox(config.SolidBackground, x => config.SolidBackground = x, "Solid Background");
        this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
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
            "Current Floor: Include all floor completion-related score up to the current floor, current character level and Aetherpool.\n" +
            "You can see your score progressively increasing each time you go to the next floor, level up or upgrade your Aetherpool.\n\n" +
            "Score Window Floor: Include all floor completion-related score up to the floor where it shows the next score window.\n\n" +
            "Last Floor: Include all floor completion-related score at once.\n\n" +
            "Floor completion-related score has nothing to do with map reveals: it will assume you are at a specific floor and Aetherpool is at max level (depending on the chosen setting) and also affect the points earned by killing enemies.\n" +
            "It's recommended to change this option before starting a fresh save file.");
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
        this.CheckBox(config.SolidBackground, x => config.SolidBackground = x, "Solid Background");
        this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
        this.ColorEdit4(config.FloorTimeColor, x => config.FloorTimeColor = x, "Floor Time");
        ImGui.SameLine();
        this.ColorEdit4(config.ScoreColor, x => config.ScoreColor = x, "Score");
        ImGui.SameLine();
        this.ColorEdit4(config.SummarySelectionColor, x => config.SummarySelectionColor = x, "Summary Selection");
    }

    private void BossStatusTimer()
    {
        var config = this.Configuration.BossStatusTimer;
        this.CheckBox(config.SolidBackground, x => config.SolidBackground = x, "Solid Background");
        this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
        this.CheckBox(config.IsStartTimeVisible, x => config.IsStartTimeVisible = x, "##IsStartTimeVisible");
        ImGui.SameLine();
        this.ColorEdit4(config.StartTimeColor, x => config.StartTimeColor = x, "Start Time");
        ImGui.SameLine();
        this.CheckBox(config.IsEndTimeVisible, x => config.IsEndTimeVisible = x, "##IsEndTimeVisible");
        ImGui.SameLine();
        this.ColorEdit4(config.EndTimeColor, x => config.EndTimeColor = x, "End Time");
        ImGui.SameLine();
        this.ColorEdit4(config.TotalTimeColor, x => config.TotalTimeColor = x, "Total Time");
    }
}