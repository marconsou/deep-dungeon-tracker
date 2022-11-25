using ImGuiNET;
using System;

namespace DeepDungeonTracker
{
    public sealed class ConfigurationWindow : WindowEx, IDisposable
    {
        private Data Data { get; }

        private string[] FieldNames { get; init; }

        public ConfigurationWindow(string id, Configuration configuration, Data data) : base(id, configuration, ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.Data = data;
            this.FieldNames = new string[] { "Kills", "Mimics", "Mandragoras", "Mimicgoras", "NPCs", "Coffers", "Enchantments", "Traps", "Deaths", "Regen Potions", "Potsherds", "Lurings", "Maps", "Time Bonuses" };
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
        }

        private void General()
        {
            var config = this.Configuration.General;
            this.CheckBox(config.ShowAccurateTargetHPPercentage, x => config.ShowAccurateTargetHPPercentage = x, "Show accurate target HP %");
            WindowEx.Tooltip("It doesn't apply to Focus Target.");
            this.CheckBox(config.SolidBackgroundWindow, x => config.SolidBackgroundWindow = x, "Force solid background to all windows");
            this.CheckBox(config.UseInGameCursor, x => config.UseInGameCursor = x, "Use in game cursor");
        }

        private void Tracker()
        {
            var config = this.Configuration.Tracker;
            this.CheckBox(config.Lock, x => config.Lock = x, "Lock");
            ImGui.SameLine();
            this.CheckBox(config.Show, x => config.Show = x, "Show");
            WindowEx.Tooltip("You need to be either inside a Deep Dungeon or in the area outside to get into it.");
            ImGui.SameLine();
            this.CheckBox(config.ShowInBetweenFloors, x => config.ShowInBetweenFloors = x, "Show in between floors");
            this.CheckBox(config.ShowFloorEffectPomanders, x => config.ShowFloorEffectPomanders = x, "Show floor effect pomanders");
            WindowEx.Tooltip("Show pomander icons (at the top left of the window) representing their effect on the current floor.");
            this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
            this.CheckBox(config.IsFloorVisible, x => config.IsFloorVisible = x, "##IsFloorVisible");
            ImGui.SameLine();
            this.ColorEdit4(config.FloorNumberColor, x => config.FloorNumberColor = x, "Floor");
            ImGui.SameLine();
            this.CheckBox(config.IsSetVisible, x => config.IsSetVisible = x, "##IsSetVisible");
            ImGui.SameLine();
            this.ColorEdit4(config.SetNumberColor, x => config.SetNumberColor = x, "Set");
            ImGui.SameLine();
            this.CheckBox(config.IsTotalVisible, x => config.IsTotalVisible = x, "##IsTotalVisible");
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
            this.CheckBox(config.Show, x => config.Show = x, "Show");
            WindowEx.Tooltip("You need to be either inside a Deep Dungeon or in the area outside to get into it.");
            ImGui.SameLine();
            this.CheckBox(config.ShowInBetweenFloors, x => config.ShowInBetweenFloors = x, "Show in between floors");
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
            this.CheckBox(config.Show, x => config.Show = x, "Show");
            WindowEx.Tooltip("You need to be either inside a Deep Dungeon or in the area outside to get into it.");
            ImGui.SameLine();
            this.CheckBox(config.ShowInBetweenFloors, x => config.ShowInBetweenFloors = x, "Show in between floors");
            this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");
            this.CheckBox(config.IsFlyTextScoreVisible, x => config.IsFlyTextScoreVisible = x, "##IsFlyTextScoreVisible");
            WindowEx.Tooltip("When the score changes, a Fly Text will be shown.");
            ImGui.SameLine();
            this.ColorEdit4(config.FlyTextScoreColor, x => config.FlyTextScoreColor = x, "Fly Text Score (experimental)");
        }

        private void Statistics()
        {
            var config = this.Configuration.Statistics;
            this.DragFloat(config.Scale, x => config.Scale = x, "Scale", 0.01f, 0.25f, 2.0f, "%.2f");

            var saveSlotSelection = this.Data.Common.SaveSlotSelection.Data;
            if (saveSlotSelection?.Count > 0)
            {
                var statistics = this.Data.Statistics;
                this.ArrowButton(statistics.FloorSetStatisticsPrevious, "##Up", ImGuiDir.Up, false);
                ImGui.SameLine();

                this.ArrowButton(statistics.FloorSetStatisticsNext, "##Down", ImGuiDir.Down, false);
                ImGui.SameLine();

                if (this.Combo(statistics.FloorSetStatistics, x => statistics.FloorSetStatistics = x, "Floors to Load").Item1)
                    statistics.DataUpdate();

                ImGui.NewLine();
                this.Button(() => statistics.Load(this.Data.Common.CurrentSaveSlot), "Load current Save Slot", false);

                foreach (var key in saveSlotSelection.Keys)
                {
                    ImGui.NewLine();
                    ImGui.Text($"{key.Replace("-", "@")}");
                    foreach (var deepDungeon in Enum.GetValues<DeepDungeon>())
                    {
                        if (deepDungeon == DeepDungeon.None)
                            continue;

                        ImGui.Text($"{deepDungeon.GetDescription()}:");
                        for (var slotNumber = 1; slotNumber <= 2; slotNumber++)
                        {
                            var fileName = DataCommon.GetSaveSlotFileName(key, new(deepDungeon, slotNumber));
                            ImGui.SameLine();
                            if (LocalStream.Exists(ServiceUtility.ConfigDirectory, fileName))
                                this.SmallButton(() => statistics.Load(fileName), $"Save Slot {slotNumber}##{fileName}", false);
                            else
                                ImGui.Text($"Save Slot {slotNumber}");
                        }
                    }
                }
            }
            else
            {
                ImGui.NewLine();
                ImGui.Text($"No save slots!");
            }
        }
    }
}