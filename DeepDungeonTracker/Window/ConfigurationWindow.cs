﻿using Dalamud.Interface;
using ImGuiNET;
using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace DeepDungeonTracker
{
    public sealed class ConfigurationWindow : WindowEx, IDisposable
    {
        private Data Data { get; }

        private Action OpenStatisticsWindow { get; }

        private string[] FieldNames { get; }

        private string SelectedBackupFileName { get; set; } = null!;

        public ConfigurationWindow(string id, Configuration configuration, Data data, Action openStatisticsWindow) : base(id, configuration, ImGuiWindowFlags.AlwaysAutoResize)
        {
            this.Data = data;
            this.OpenStatisticsWindow = openStatisticsWindow;
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
                if (ImGui.BeginTabItem("Information"))
                {
                    this.Information();
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
            this.CheckBox(config.UseInGameCursor, x => config.UseInGameCursor = x, "Use in game cursor");
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
            if (config.FontType != FontType.Default)
            {
                ImGui.SameLine();
                this.CheckBox(config.FontEnlarge, x => config.FontEnlarge = x, "Enlarge");
            }
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
                "Current Floor: Include all floor completion-related score up to the current floor and current character level.\nYou can see your score progressively increasing each time you go to the next floor and level up.\n\n" +
                "Score Window Floor: Include all floor completion-related score up to the floor where it shows the next score window.\n\n" +
                "Last Floor: Include all floor completion-related score.");
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
            ImGui.NewLine();

            var saveSlotSelection = this.Data.Common.SaveSlotSelection.GetData();
            if (saveSlotSelection?.Count > 0)
            {
                this.IconButton(statistics.FloorSetStatisticsSummary, FontAwesomeIcon.AngleDoubleLeft, "Summary");

                ImGui.SameLine();
                this.IconButton(statistics.FloorSetStatisticsPrevious, FontAwesomeIcon.AngleLeft, "Left");

                ImGui.SameLine();
                this.IconButton(statistics.FloorSetStatisticsNext, FontAwesomeIcon.AngleRight, "Right");

                ImGui.SameLine();
                this.IconButton(statistics.FloorSetStatisticsCurrent, FontAwesomeIcon.AngleDoubleRight, "Current");

                ImGui.SameLine();
                if (this.Combo(statistics.FloorSetStatistics, x => statistics.FloorSetStatistics = x, "##FloorSetStatistics").Item1)
                    statistics.DataUpdate();

                foreach (var saveSlot in saveSlotSelection)
                {
                    ImGui.NewLine();
                    WindowEx.Child(() =>
                    {
                        saveSlotSelection.TryGetValue(saveSlot.Key, out var saveSlotSelectionData);
                        ImGui.Text($"{saveSlot.Key.Replace("-", "@", StringComparison.InvariantCultureIgnoreCase)}");
                        ImGui.Dummy(new(0.0f, 4.0f));
                        foreach (var deepDungeon in Enum.GetValues<DeepDungeon>())
                        {
                            if (deepDungeon == DeepDungeon.None)
                                continue;

                            if (ImGui.BeginTable($"{saveSlot.Key}Table", 3))
                            {
                                ImGui.TableNextRow();
                                ImGui.TableNextColumn();
                                ImGui.Text($"{deepDungeon.GetDescription()}:");
                                for (var saveSlotNumber = 1; saveSlotNumber <= 2; saveSlotNumber++)
                                {
                                    var fileName = DataCommon.GetSaveSlotFileName(saveSlot.Key, new(deepDungeon, saveSlotNumber));

                                    ImGui.TableNextColumn();
                                    var enableButtons =
                                        (!this.Data.IsInsideDeepDungeon && LocalStream.Exists(ServiceUtility.ConfigDirectory, fileName)) ||
                                        (this.Data.IsInsideDeepDungeon && this.Data.Common.GetSaveSlotFileName(saveSlotSelectionData ?? new()) == fileName);

                                    WindowEx.Disabled(() =>
                                    {
                                        this.IconButton(() =>
                                        {
                                            LocalStream.Copy(ServiceUtility.ConfigDirectory, Directories.Backups, fileName, $"{LocalStream.FormatFileName(fileName, false)} {DateTime.Now.ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture)}.json".Trim());
                                        }, FontAwesomeIcon.Clone, $"{fileName}Clone");
                                        ImGui.SameLine();
                                        this.Button(() =>
                                        {
                                            if (!this.Data.IsInsideDeepDungeon)
                                                this.Data.Common.LoadDeepDungeonData(false, saveSlot.Key, new(deepDungeon, saveSlotNumber));

                                            statistics.Load(this.Data.Common.CurrentSaveSlot, this.OpenStatisticsWindow);
                                        }, $"Save Slot {saveSlotNumber}##{fileName}");
                                    }, !enableButtons);
                                }
                                ImGui.EndTable();
                            }
                        }
                    }, $"{saveSlot.Key}Child", 364.0f, 135.0f);
                    ImGui.Separator();
                }
            }
            else
                ImGui.TextColored(Color.Gray, "No save slots!");

            ImGui.NewLine();

            this.IconButton(() => { LocalStream.OpenFolder(Directories.Backups); }, FontAwesomeIcon.FolderOpen, "BackupsFolderOpen");
            ImGui.SameLine();
            ImGui.Text("Backups");
            this.IconButton(() => { LocalStream.OpenFolder(Directories.Screenshots); }, FontAwesomeIcon.FolderOpen, "ScreenshotsFolderOpen");
            ImGui.SameLine();
            ImGui.Text("Screenshots");
            var fileNames = LocalStream.GetFileNamesFromDirectory(Directories.Backups).Where(x => LocalStream.IsExtension(x, ".json")).ToArray();
            if (fileNames.Length > 0)
            {
                ImGui.Dummy(new(0.0f, 4.0f));
                WindowEx.Child(() =>
                {
                    var deleteDialog = "Delete Dialog##Deep Dungeon Tracker";
                    foreach (var fileName in fileNames)
                    {
                        var id = LocalStream.FormatFileName(fileName, true);
                        var formattedFileName = $"{LocalStream.FormatFileName(fileName, false)}";
                        WindowEx.Disabled(() =>
                        {
                            this.IconButton(() =>
                            {
                                ImGui.OpenPopup(deleteDialog);
                                this.SelectedBackupFileName = formattedFileName;
                            }, FontAwesomeIcon.Trash, $"{id}Trash");
                            ImGui.SameLine();
                            this.Button(() =>
                            {
                                if (!this.Data.IsInsideDeepDungeon)
                                    this.Data.Common.LoadDeepDungeonData(false, fileName);

                                statistics.Load(this.Data.Common.CurrentSaveSlot, this.OpenStatisticsWindow);
                            }, formattedFileName);
                        }, this.Data.IsInsideDeepDungeon);
                    }

                    var visibility = true;
                    if (ImGui.BeginPopupModal(deleteDialog, ref visibility, ImGuiWindowFlags.AlwaysAutoResize))
                    {
                        ImGui.Text("Are you sure you want to delete this item?");
                        ImGui.Text($"{this.SelectedBackupFileName}");
                        ImGui.Separator();
                        this.Button(() => { LocalStream.Delete(Directories.Backups, $"{this.SelectedBackupFileName}.json"); ImGui.CloseCurrentPopup(); }, "Confirm");
                        ImGui.SameLine();
                        this.Button(() => { ImGui.CloseCurrentPopup(); }, "Cancel");
                        ImGui.EndPopup();
                    }
                }, "Backups", 364.0f, (fileNames.Length + 1) * 27.0f);
            }
            else
                ImGui.TextColored(Color.Gray, "No backups!");

            this.ConvertSaveUtility();
        }

        private void ConvertSaveUtility()
        {
            var statistics = this.Data.Statistics;

            if (ImGui.CollapsingHeader("Convert Save Utility"))
            {
                ImGui.TextWrapped(
                    "Any treasure you get from coffers (pomanders, magicites, potsherds, etc.) has a specific Id (Pomander of Safety = 0, Pomander of Sight = 1, etc.), " +
                    "and due to the new pomanders coming in the new Deep Dungeon: Eureka Orthos, some of those Ids need to be shifted to new values to free up space for the new pomanders." +
                    "\n\nThis will only affect treasures that you got from Silver Coffers (magicites and aetherpool upgrades) and Bronze Coffers (potsherds and medicines), for all save files up to this update. " +
                    "This will not affect the score or anything else." +
                    "\n\nSo, for old save files, the Id for Inferno Magicite is 19. When you use this utility, the new id for Inferno Magicite will be 60, and when Eureka Orthos becomes available, " +
                    "the first newly added pomander will take the Id of 19, and so on. This utility will help convert those Ids, and it will be available until Eureka Orthos's release. " +
                    "This utility is not needed for Eureka Orthos; it's just to fix old save file Ids." +
                    "\n\nIf you open an old or ongoing save file to analyze some data before converting it, some coffer-related icons will be shown improperly (grayed icon)." +
                    "\nFresh save files will not be affected by this issue." +
                    "\nYou need to backup your file before using this utility.");
                ImGui.Dummy(new(0.0f, 4.0f));
                ImGui.TextColored(Color.Yellow, "TL;DR: If you see a grayed icon while analyzing some data, you need to convert your saved file.");

                var fileNames = LocalStream.GetFileNamesFromDirectory(Directories.Backups).Where(x => LocalStream.IsExtension(x, ".json")).ToArray();
                if (fileNames.Length > 0)
                {
                    ImGui.Dummy(new(0.0f, 4.0f));
                    WindowEx.Child(() =>
                    {
                        foreach (var fileName in fileNames)
                        {
                            var id = LocalStream.FormatFileName(fileName, true);
                            var formattedFileName = $"{LocalStream.FormatFileName(fileName, false)}";
                            WindowEx.Disabled(() =>
                            {
                                this.Button(() =>
                                {
                                    var saveSlot = LocalStream.Load<SaveSlot>(Directories.Backups, id);

                                    foreach (var floorSet in saveSlot?.FloorSets ?? Enumerable.Empty<FloorSet>())
                                    {
                                        foreach (var floor in floorSet.Floors)
                                        {
                                            var coffers = floor.Coffers.ToList();
                                            floor.Coffers.Clear();

                                            foreach (var coffer in coffers)
                                            {
                                                if (coffer == Coffer.DeprecatedInfernoMagicite)
                                                    floor.Coffers.Add(Coffer.InfernoMagicite);
                                                else if (coffer == Coffer.DeprecatedCragMagicite)
                                                    floor.Coffers.Add(Coffer.CragMagicite);
                                                else if (coffer == Coffer.DeprecatedVortexMagicite)
                                                    floor.Coffers.Add(Coffer.VortexMagicite);
                                                else if (coffer == Coffer.DeprecatedElderMagicite)
                                                    floor.Coffers.Add(Coffer.ElderMagicite);
                                                else if (coffer == Coffer.DeprecatedAetherpool)
                                                    floor.Coffers.Add(Coffer.Aetherpool);
                                                else if (coffer == Coffer.DeprecatedPotsherd)
                                                    floor.Coffers.Add(Coffer.Potsherd);
                                                else if (coffer == Coffer.DeprecatedMedicine)
                                                    floor.Coffers.Add(Coffer.Medicine);
                                                else
                                                    floor.Coffers.Add(coffer);
                                            }

                                            var pomanders = floor.Pomanders.ToList();
                                            floor.Pomanders.Clear();

                                            foreach (var pomander in pomanders)
                                            {
                                                if (pomander == Pomander.DeprecatedInfernoMagicite)
                                                    floor.Pomanders.Add(Pomander.InfernoMagicite);
                                                else if (pomander == Pomander.DeprecatedCragMagicite)
                                                    floor.Pomanders.Add(Pomander.CragMagicite);
                                                else if (pomander == Pomander.DeprecatedVortexMagicite)
                                                    floor.Pomanders.Add(Pomander.VortexMagicite);
                                                else if (pomander == Pomander.DeprecatedElderMagicite)
                                                    floor.Pomanders.Add(Pomander.ElderMagicite);
                                                else
                                                    floor.Pomanders.Add(pomander);
                                            }
                                        }
                                    }

                                    LocalStream.Save(Directories.Backups, $"{formattedFileName} (Converted).json", saveSlot).ConfigureAwait(true);

                                }, formattedFileName);
                            }, this.Data.IsInsideDeepDungeon);
                        }
                    }, "Convert Save Utility", 364.0f, (fileNames.Length + 1) * 27.0f);
                }
                else
                    ImGui.TextColored(Color.Gray, "No backups!");
            }
        }

        private void Information()
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
            if (ImGui.CollapsingHeader("OpCodes"))
            {
                ImGui.TextWrapped("Values in this section should not be zero.");
                ImGui.TextWrapped($"{JsonSerializer.Serialize(this.Configuration.OpCodes, new JsonSerializerOptions() { WriteIndented = true, })}");
            }
        }
    }
}