﻿using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Action = System.Action;

namespace DeepDungeonTracker;

public sealed class MainWindow : WindowEx, IDisposable
{
    private Data Data { get; }

    private Action OpenStatisticsWindow { get; }

    private int SaveSlotIndex { get; set; }

    private int BackupIndex { get; set; }

    private int PreviousBackupFileTotal { get; set; }

    private string SelectedBackupFileName { get; set; } = null!;

    private OpenFolderButton BackupFolderButton { get; } = new();

    private ArrowButton SaveSlotArrowButtonPrevious { get; } = new(false);

    private ArrowButton SaveSlotArrowButtonNext { get; } = new(true);

    private ArrowButton BackupArrowButtonPrevious { get; } = new(false);

    private ArrowButton BackupSlotArrowButtonNext { get; } = new(true);

    private CloseButton CloseButton { get; } = new();

    private List<TextButton> SaveSlotButtons { get; } = [];

    private List<BackupButton> SaveSlotBackupButtons { get; } = [];

    private List<TextButton> LastSaveButtons { get; } = [];

    private List<BackupButton> LastSaveBackupButtons { get; } = [];

    private List<TextButton> BackupFileButtons { get; } = [];

    private List<DeleteButton> BackupDeleteButtons { get; } = [];

    private static int BackupFilesPerPage => 10;

    private static string[] BackupFileNames => LocalStream.GetFileNamesFromDirectory(Directories.Backups).Where(x => LocalStream.IsExtension(x, ".json")).ToArray();

    public MainWindow(string id, Configuration configuration, Data data, Action openStatisticsWindow) : base(id, configuration, WindowEx.StaticNoBackgroundMoveInputs)
    {
        this.Data = data;
        this.OpenStatisticsWindow = openStatisticsWindow;

        var saveSlotsTotal = 2 * (Enum.GetValues(typeof(DeepDungeon)).Length - 1);
        for (var i = 0; i < saveSlotsTotal; i++)
        {
            this.SaveSlotButtons.Add(new());
            this.SaveSlotBackupButtons.Add(new());
            this.LastSaveButtons.Add(new());
            this.LastSaveBackupButtons.Add(new());
        }

        for (var i = 0; i < MainWindow.BackupFilesPerPage; i++)
        {
            this.BackupFileButtons.Add(new());
            this.BackupDeleteButtons.Add(new());
        }
    }

    public void Dispose() { }

    public void CheckForEvents()
    {
        void BackupPrevious()
        {
            this.BackupIndex--;
            if (this.BackupIndex < 0)
                this.BackupIndex = 0;
        }

        void BackupNext()
        {
            this.BackupIndex++;
            var max = MainWindow.BackupFileNames.Length - MainWindow.BackupFilesPerPage;
            if (this.BackupIndex > max)
                this.BackupIndex = max;
        }

        if (this.SaveSlotArrowButtonPrevious.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            this.SaveSlotIndex--;
            if (this.SaveSlotIndex <= -1)
                this.SaveSlotIndex = this.Data.Common.SaveSlotSelection.GetData().Count - 1;
        }
        else if (this.SaveSlotArrowButtonNext.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            this.SaveSlotIndex++;
            if (this.SaveSlotIndex >= this.Data.Common.SaveSlotSelection.GetData().Count)
                this.SaveSlotIndex = 0;
        }
        if (this.BackupArrowButtonPrevious.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            BackupPrevious();
        }
        else if (this.BackupSlotArrowButtonNext.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            BackupNext();
        }
        else if (this.BackupFolderButton.OnMouseLeftClickRelease())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            LocalStream.OpenFolder(Directories.Backups);
        }
        else if (this.CloseButton.OnMouseLeftClick())
            this.IsOpen = false;

        if (ImGui.IsWindowHovered())
        {
            var io = ImGui.GetIO();
            if (io.MouseWheel > 0)
                BackupPrevious();
            else if (io.MouseWheel < 0)
                BackupNext();
        }
    }

    public override void OnOpen() => this.Data.Audio.PlaySound(SoundIndex.OnOpenMenu);

    public override void OnClose() => this.Data.Audio.PlaySound(SoundIndex.OnCloseMenu);

    private void SaveSlots(DataUI ui, DataAudio audio, float width, float height)
    {
        var centerX = width / 2.0f;
        var centerY = height / 2.0f;
        var saveSlotSelection = this.Data.Common.SaveSlotSelection.GetData();

        this.SaveSlotArrowButtonPrevious.Position = new(centerX - 100.0f, 43.0f);
        this.SaveSlotArrowButtonNext.Position = new(centerX + 64.0f, 43.0f);

        ui.DrawTextMiedingerMid(centerX, 56.0f, "Save Slots", Color.White, Alignment.Center);

        if (saveSlotSelection?.Count > 0)
        {
            if (saveSlotSelection.Count > 1)
            {
                this.SaveSlotArrowButtonPrevious.Draw(ui, audio);
                this.SaveSlotArrowButtonNext.Draw(ui, audio);
            }

            var saveSlot = saveSlotSelection.ElementAt(this.SaveSlotIndex);

            saveSlotSelection.TryGetValue(saveSlot.Key, out var saveSlotSelectionData);
            ui.DrawTextAxis(centerX, 83.0f, $"{saveSlot.Key.Replace("-", "  (", StringComparison.InvariantCultureIgnoreCase)})", Color.White, Alignment.Center);

            void Buttons(TextButton mainButton, BackupButton backupButton, float x, float y, string saveSlotText, KeyValuePair<string, SaveSlotSelection.SaveSlotSelectionData> saveSlot, DeepDungeon deepDungeon, int saveSlotNumber, string fileName, bool enableButtons)
            {
                ui.DrawTextAxis(x, y, saveSlotText, enableButtons ? Color.White : Color.Gray);

                mainButton.Position = new(x, y);
                mainButton.Size = ui.GetAxisTextSize(saveSlotText);

                backupButton.Position = new(x - 35.0f, y - 3.0f);

                if (enableButtons)
                {
                    mainButton.Draw(ui, audio);
                    backupButton.Draw(ui, audio);

                    if (mainButton.OnMouseLeftClick())
                    {
                        this.Data.Audio.PlaySound(SoundIndex.OnClick);

                        if (!this.Data.IsInsideDeepDungeon)
                            this.Data.Common.LoadDeepDungeonData(false, fileName);

                        this.Data.Statistics.Load(this.Data.Common.CurrentSaveSlot, this.OpenStatisticsWindow);
                    }
                    else if (backupButton.OnMouseLeftClick())
                    {
                        this.Data.Audio.PlaySound(SoundIndex.OnClick);
                        var destFileName = $"{LocalStream.FormatFileName(fileName, false)} {DateTime.Now.ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture)}.json".Trim();
                        if (LocalStream.Copy(ServiceUtility.ConfigDirectory, Directories.Backups, fileName, destFileName))
                            Service.ChatGui.Print($"A new file has been created! ({destFileName})");
                        else
                            Service.ChatGui.Print($"No file created!");
                    }
                }
            }

            foreach (var deepDungeon in Enum.GetValues<DeepDungeon>())
            {
                if (deepDungeon == DeepDungeon.None)
                    continue;

                var x = centerX;
                var y = ((int)deepDungeon * 100.0f) + 10.0f;

                ui.DrawDeepDungeon(x, y, deepDungeon, Alignment.Center);
                x -= centerX / 2.0f;
                y += 25.0f;

                for (var saveSlotNumber = 1; saveSlotNumber <= 2; saveSlotNumber++)
                {
                    var buttonIndex = (((int)deepDungeon - 1) * 2) + (saveSlotNumber - 1);

                    var saveSlotfileName = DataCommon.GetSaveSlotFileName(saveSlot.Key, new(deepDungeon, saveSlotNumber));
                    var lastSaveFileName = DataCommon.GetLastSaveFileName(saveSlot.Key, new(deepDungeon, saveSlotNumber));

                    var saveSlotEnableButtons =
                            (!this.Data.IsInsideDeepDungeon && LocalStream.Exists(ServiceUtility.ConfigDirectory, saveSlotfileName)) ||
                            (this.Data.IsInsideDeepDungeon && this.Data.Common.GetSaveSlotFileName(saveSlotSelectionData ?? new()) == saveSlotfileName);

                    var lastSaveEnableButtons =
                           (!this.Data.IsInsideDeepDungeon && LocalStream.Exists(ServiceUtility.ConfigDirectory, lastSaveFileName));

                    Buttons(this.SaveSlotButtons[buttonIndex], this.SaveSlotBackupButtons[buttonIndex], x, y, $"Save Slot {saveSlotNumber}", saveSlot, deepDungeon, saveSlotNumber, saveSlotfileName, saveSlotEnableButtons);
                    Buttons(this.LastSaveButtons[buttonIndex], this.LastSaveBackupButtons[buttonIndex], x, y + 30.0f, "Last Save", saveSlot, deepDungeon, saveSlotNumber, lastSaveFileName, lastSaveEnableButtons);

                    x += width / 3.0f;
                }
            }
        }
        else
            ui.DrawTextAxis(centerX, (centerY / 2.0f) + 15.0f, "No save files!", Color.White, Alignment.Center);
    }

    private void Backups(DataUI ui, DataAudio audio, float width, float height)
    {
        var centerX = width / 2.0f;
        var centerY = height / 2.0f;
        this.BackupArrowButtonPrevious.Position = new(centerX - 100.0f, centerY + 9.0f);
        this.BackupSlotArrowButtonNext.Position = new(centerX + 64.0f, centerY + 9.0f);

        this.BackupFolderButton.Position = new(width - 50.0f, (height / 2.0f) + 10.0f);
        this.BackupFolderButton.Draw(ui, audio);

        ui.DrawTextMiedingerMid(centerX, centerY + 23.0f, "Backups", Color.White, Alignment.Center);

        var fileNames = LocalStream.GetFileNamesFromDirectory(Directories.Backups).Where(x => LocalStream.IsExtension(x, ".json")).OrderBy(x => x).ToArray();
        if (fileNames.Length > 0)
        {
            foreach (var button in this.BackupFileButtons)
                button.Show = false;

            foreach (var button in this.BackupDeleteButtons)
                button.Show = false;

            var deleteDialog = "Delete Dialog##Deep Dungeon Tracker";
            var x = 60.0f;
            var y = centerY + 50.0f;
            var buttonIndex = 0;
            var textWidthCap = width - x - 25.0f;

            foreach (var fileName in fileNames.Skip(this.BackupIndex).Take(MainWindow.BackupFilesPerPage))
            {
                var enableButtons = !this.Data.IsInsideDeepDungeon;
                var formattedFileName = $"{LocalStream.FormatFileName(fileName, false)}";
                var uiFileName = string.Empty;
                var fileNameWidthTotal = 0.0f;
                var textCapped = false;
                foreach (var item in formattedFileName)
                {
                    fileNameWidthTotal += ui.GetAxisTextSize($"{item}").X;
                    if (fileNameWidthTotal < textWidthCap)
                        uiFileName += item;
                    else
                    {
                        textCapped = true;
                        break;
                    }
                }
                if (textCapped)
                    uiFileName += "...";

                ui.DrawTextAxis(x, y, uiFileName, enableButtons ? Color.White : Color.Gray);

                var backupFileButton = this.BackupFileButtons[buttonIndex];

                backupFileButton.Position = new(x, y);
                backupFileButton.Size = ui.GetAxisTextSize(uiFileName);

                if (enableButtons)
                {
                    backupFileButton.Show = true;
                    backupFileButton.Draw(ui, audio);

                    if (backupFileButton.OnMouseLeftClick())
                    {
                        this.Data.Audio.PlaySound(SoundIndex.OnClick);

                        if (!this.Data.IsInsideDeepDungeon)
                            this.Data.Common.LoadDeepDungeonData(false, fileName);

                        this.Data.Statistics.Load(this.Data.Common.CurrentSaveSlot, this.OpenStatisticsWindow);
                    }
                }

                var backupDeleteButton = this.BackupDeleteButtons[buttonIndex];

                backupDeleteButton.Show = true;
                backupDeleteButton.Position = new(x - backupDeleteButton.Size.X - 15.0f, y + (backupFileButton.Size.Y / 2.0f) - (backupDeleteButton.Size.Y / 2.0f));
                backupDeleteButton.Draw(ui, audio);

                if (backupDeleteButton.OnMouseLeftClick())
                {
                    this.Data.Audio.PlaySound(SoundIndex.OnClick);
                    this.SelectedBackupFileName = formattedFileName;
                    ImGui.OpenPopup(deleteDialog);
                }

                y += 34.0f;
                buttonIndex++;
            }

            if (fileNames.Length > MainWindow.BackupFilesPerPage)
            {
                this.BackupArrowButtonPrevious.Draw(ui, audio);
                this.BackupSlotArrowButtonNext.Draw(ui, audio);
            }

            this.ModalWindow(deleteDialog);
        }
        else
            ui.DrawTextAxis(centerX, centerY + (centerY / 2.0f) + 15.0f, "No backups!", Color.White, Alignment.Center);
    }

    private void ModalWindow(string deleteDialog)
    {
        var visibility = true;
        var style = ImGui.GetStyle();
        var padding = style.WindowPadding;
        style.WindowPadding = new(5.0f, 5.0f);
        if (ImGui.BeginPopupModal(deleteDialog, ref visibility, ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.Text("Are you sure you want to delete this item?");
            ImGui.Text($"{this.SelectedBackupFileName}");
            ImGui.Separator();
            this.Button(() =>
            {
                this.Data.Audio.PlaySound(SoundIndex.OnClick);
                var deletedFileName = $"{this.SelectedBackupFileName}.json";
                LocalStream.Delete(Directories.Backups, deletedFileName);
                Service.ChatGui.Print($"A file has been deleted! ({deletedFileName})");
                ImGui.CloseCurrentPopup();
            }, "Confirm");
            ImGui.SameLine();
            this.Button(() =>
            {
                this.Data.Audio.PlaySound(SoundIndex.OnCloseMenu);
                ImGui.CloseCurrentPopup();
            }, "Cancel");
            ImGui.EndPopup();
        }
        style.WindowPadding = padding;
    }

    public override void PreOpenCheck()
    {
        var currentBackupFileTotal = MainWindow.BackupFileNames.Length;

        if (currentBackupFileTotal > this.PreviousBackupFileTotal)
            this.BackupIndex = 0;
        else if (currentBackupFileTotal < this.PreviousBackupFileTotal)
        {
            this.BackupIndex += currentBackupFileTotal - this.PreviousBackupFileTotal;
            if (this.BackupIndex < 0)
                this.BackupIndex = 0;
        }

        this.PreviousBackupFileTotal = currentBackupFileTotal;
    }

    public override void Draw()
    {
        var ui = this.Data.UI;
        var config = this.Configuration.Main;
        var audio = this.Data.Audio;
        ui.Scale = config.Scale;

        var width = 440.0f;
        var height = 800.0f;

        ui.DrawBackground(width, height, (!config.SolidBackground && this.IsFocused) || config.SolidBackground);
        ui.DrawDivisorHorizontal(14.0f, 34.0f, width - 26.0f);
        ui.DrawDivisorHorizontal(14.0f, height / 2.0f, width - 26.0f);
        ui.DrawTextTrumpGothic(15.0f, 5.0f, "Save Slots & Backups", new(0.8197f, 0.8197f, 0.8197f, 1.0f), Alignment.Left);

        this.CloseButton.Position = new(width - 35.0f, 7.0f);
        this.CloseButton.Draw(ui, audio);

        this.SaveSlots(ui, audio, width, height);
        this.Backups(ui, audio, width, height);

        this.WindowSizeUpdate(width, height, ui.Scale);
        this.CheckForEvents();
    }
}