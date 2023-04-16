using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using static DeepDungeonTracker.DataStatistics;

namespace DeepDungeonTracker;

public sealed class BossStatusTimerWindow : WindowEx, IDisposable
{
    private Data Data { get; }

    private ScreenshotButton ScreenshotButton { get; } = new();

    private OpenFolderButton ScreenshotFolderButton { get; } = new();

    private CloseButton CloseButton { get; } = new();

    public BossStatusTimerWindow(string id, Configuration configuration, Data data) : base(id, configuration, WindowEx.StaticNoBackgroundMoveInputs) => this.Data = data;

    public void Dispose() { }

    private string GetFloorNumber()
    {
        var statistics = this.Data.Statistics;
        if (statistics.FloorSetStatistics != FloorSetStatistics.Summary)
        {
            if (statistics.IsEurekaOrthosFloor99)
                return "99";
            else
                return statistics.FloorSetStatistics.GetDescription().Split('-').LastOrDefault() ?? string.Empty;
        }
        return string.Empty;
    }

    public void CheckForEvents()
    {
        if (this.ScreenshotButton.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.Screenshot);
            var fileName = $"Boss Status Timer {this.GetFloorNumber()} {DateTime.Now.ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture)}.png".Trim();
            var fontGlobalScale = ImGui.GetIO().FontGlobalScale;
            var size = this.GetSizeScaled() * (fontGlobalScale > 1.0f ? fontGlobalScale : 1.0f);
            var result = ScreenStream.Screenshot(ImGui.GetWindowPos(), size, Directories.Screenshots, fileName);
            Service.ChatGui.Print(result.Item1 ? $"{result.Item2} ({fileName})" : result.Item2);
        }
        else if (this.ScreenshotFolderButton.OnMouseLeftClickRelease())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            LocalStream.OpenFolder(Directories.Screenshots);
        }
        else if (this.CloseButton.OnMouseLeftClick())
            this.IsOpen = false;
    }

    public override void OnOpen() => this.Data.Audio.PlaySound(SoundIndex.OnOpenMenu);

    public override void OnClose() => this.Data.Audio.PlaySound(SoundIndex.OnCloseMenu);

    private void DrawPomanderIcon(float x, float y, float iconSize, IEnumerable<StatisticsItem<Pomander>>? data)
    {
        var offset = 4.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Pomander>>())
        {
            var pomander = (Pomander)(Enum)item.Value;
            this.Data.UI.DrawPomander(x + offset, y + offset, pomander);
            x += iconSize;
        }
    }

    private void DrawPomanderText(float x, float y, float iconSize, IEnumerable<StatisticsItem<Pomander>>? data)
    {
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Pomander>>())
        {
            var pomander = (Pomander)(Enum)item.Value;
            var total = item.Total;
            if (total > 1)
                this.Data.UI.DrawTextAxis(x + iconSize, y + iconSize, total.ToString(CultureInfo.InvariantCulture), Color.White, Alignment.Right);
            x += iconSize;
        }
    }

    private void DrawWindowHeader(float left, float top, float width, float headerHeight)
    {
        var ui = this.Data.UI;
        var statistics = this.Data.Statistics;

        ui.DrawDeepDungeon(width / 2.0f, top + 5.0f, statistics.DeepDungeon, Alignment.Center);
        top += headerHeight - 32.0f;

        var x = left + 15.0f;
        var y = top + 5.0f;

        if (ClassJobIds.Data.TryGetValue(statistics.ClassJobId, out var classJobId))
            ui.DrawJob(x, y, classJobId.Item1, 0.25f);
        else
            ui.DrawJob(x, y, classJobId.Item1, 0.25f);

        x += 20.0f;
        y -= 22.0f;
        var iconSize = 48.0f;

        var pomanders = statistics.PomandersBossStatusTimer;
        this.DrawPomanderIcon(x, y, iconSize, pomanders);
        this.DrawPomanderText(x, y, iconSize, pomanders);
    }

    private void DrawWindowContent(BossStatusTimerData data, float left, float top, float iconSize, float offsetY, float windowWidth)
    {
        if (data == null)
            return;

        var ui = this.Data.UI;
        var config = this.Configuration.BossStatusTimer;

        var x = left;
        var y = top;
        var offsetX = 5.0f;
        var barStartX = left + iconSize + offsetX;
        var barWidth = 320.0f;
        var barHeight = 18.0f;
        TimeSpan totalTime;

        void DrawIcon(BossStatusTimer bossStatusTimer)
        {
            y += bossStatusTimer != BossStatusTimer.Combat ? iconSize + offsetY : 0.0f;
            totalTime = new();
            ui.DrawBossStatusTimer(x, y, bossStatusTimer);
        }

        void DrawBarAndTextCurrent(BossStatusTimerItem currentItem, float iconY)
        {
            var y = iconY + (iconSize / 2.0f) - (barHeight / 2.0f);
            var start = (currentItem.Start - data.Combat.Start).Round();
            var end = (currentItem.End - data.Combat.Start).Round();
            var durationTime = ((currentItem.End - data.Combat.Start) - (currentItem.Start - data.Combat.Start)).Round();
            totalTime += durationTime;
            var combatDuration = data.Combat.Duration();
            var x = (float)(barStartX + ((start.TotalSeconds / combatDuration.TotalSeconds) * barWidth));
            var width = (float)(durationTime.TotalSeconds / combatDuration.TotalSeconds) * barWidth;

            var offsetX = 8.0f;

            ui.DrawBar(x + offsetX, y, width - (offsetX * 2.0f), barHeight);
            if (currentItem.BossStatusTimer != BossStatusTimer.Combat)
            {
                if (config.IsStartTimeVisible && start != default && start != combatDuration)
                {
                    var timeText = $"{start:mm\\:ss}";
                    var timeTextSize = ui.GetAxisTextSize(timeText);
                    ui.DrawArrow(x - 3.5f, y - 3.5f, false);
                    ui.DrawTextAxis(x - (timeTextSize.X / 2.0f), y - timeTextSize.Y, timeText, config.StartTimeColor);
                }
                if (config.IsEndTimeVisible && end != default && end != combatDuration)
                {
                    var timeText = $"{end:mm\\:ss}";
                    var timeTextSize = ui.GetAxisTextSize(timeText);
                    ui.DrawArrow(x + width - 3.5f, y + barHeight - 3.5f, true);
                    ui.DrawTextAxis(x + width - (timeTextSize.X / 2.0f), y + barHeight, timeText, config.EndTimeColor);
                }
            }
        }

        void DrawTextTotal(float y, TimeSpan totalTime)
        {
            var text = $"{totalTime:mm\\:ss}";
            ui.DrawTextAxis(barStartX + barWidth + offsetX, y + (iconSize / 2.0f) - (ui.GetAxisTextSize(text).Y / 2.0f), text, config.TotalTimeColor);
        }

        void Draw(BossStatusTimer bossStatusTimer, ICollection<BossStatusTimerItem> data)
        {
            var newData = BossStatusTimerData.RemoveShortDuration(data);
            if (newData.Count == 0)
                return;

            DrawIcon(bossStatusTimer);
            foreach (var item in newData)
                DrawBarAndTextCurrent(item, y);
            DrawTextTotal(y, totalTime);
        }

        ui.DrawTextMiedingerMid(windowWidth - 48.0f, top - 5.0f, "Total", Color.White, Alignment.Center);

        DrawIcon(BossStatusTimer.Combat);
        DrawBarAndTextCurrent(data.Combat, y);
        DrawTextTotal(y, totalTime);

        Draw(BossStatusTimer.Medicated, data.Medicated);
        Draw(BossStatusTimer.DamageUp, data.DamageUp);
        Draw(BossStatusTimer.DamageUpHeavenOnHigh, data.DamageUpHeavenOnHigh);
        Draw(BossStatusTimer.DamageUpEurekaOrthos, data.DamageUpEurekaOrthos);
        Draw(BossStatusTimer.VulnerabilityDown, data.VulnerabilityDown);
        Draw(BossStatusTimer.VulnerabilityDownHeavenOnHigh, data.VulnerabilityDownHeavenOnHigh);
        Draw(BossStatusTimer.VulnerabilityDownEurekaOrthos, data.VulnerabilityDownEurekaOrthos);
        Draw(BossStatusTimer.RehabilitationHeavenOnHigh, data.RehabilitationHeavenOnHigh);
        Draw(BossStatusTimer.RehabilitationEurekaOrthos, data.RehabilitationEurekaOrthos);

        if (data.VulnerabilityUp.Count > 0)
        {
            var vulnerabilityUps = data.VulnerabilityUp.GroupBy(x => x.Stacks).OrderByDescending(x => x.Key).ToList();
            foreach (var vulnerabilityUpByStack in vulnerabilityUps)
            {
                DrawIcon(BossStatusTimer.VulnerabilityUp + vulnerabilityUpByStack.Key - 1);
                foreach (var item in vulnerabilityUpByStack)
                    DrawBarAndTextCurrent(item, y);
                DrawTextTotal(y, totalTime);
            }
        }

        Draw(BossStatusTimer.Enervation, data.Enervation);
        Draw(BossStatusTimer.AccursedPox, data.AccursedPox);
        Draw(BossStatusTimer.Weakness, data.Weakness);
        Draw(BossStatusTimer.BrinkOfDeath, data.BrinkOfDeath);
    }

    public override void Draw()
    {
        var ui = this.Data.UI;
        var config = this.Configuration.BossStatusTimer;
        var audio = this.Data.Audio;
        ui.Scale = config.Scale;
        var statistics = this.Data.Statistics;
        var data = statistics.FloorSet?.BossStatusTimerData;
        var isDataValid = data?.Combat.IsDataValid() ?? false;

        var left = 15.0f;
        var top = 50.0f;
        var headerHeight = 70.0f;
        var iconSize = 32.0f;
        var offsetY = 24.0f;
        var width = 440.0f;
        var height = top + headerHeight + ((isDataValid ? (data?.TimersCount() ?? 1) : 1) * (iconSize + offsetY)) - 5.0f;
        var floorNumber = this.GetFloorNumber();

        ui.DrawBackground(width, height, (!config.SolidBackground && this.IsFocused) || config.SolidBackground);
        ui.DrawDivisorHorizontal(14.0f, 34.0f, width - 26.0f);
        ui.DrawTextTrumpGothic(15.0f, 5.0f, "Boss Status Timer", new(0.8197f, 0.8197f, 0.8197f, 1.0f), Alignment.Left);
        ui.DrawTextTrumpGothic(width - 38.0f, 38.0f, floorNumber, new(0.8197f, 0.8197f, 0.8197f, 1.0f), Alignment.Right);

        this.ScreenshotButton.Position = new Vector2(138.0f, 7.0f);
        this.ScreenshotFolderButton.Position = new Vector2(173.0f, 7.0f);
        this.CloseButton.Position = new(width - 35.0f, 7.0f);

        this.ScreenshotButton.Draw(ui, audio);
        this.ScreenshotFolderButton.Draw(ui, audio);
        this.CloseButton.Draw(ui, audio);

        if (data != null && isDataValid)
        {
            this.DrawWindowHeader(left, top, width, headerHeight);
            top += headerHeight;
            this.DrawWindowContent(data, left, top, iconSize, offsetY, width);
        }
        else
            ui.DrawTextAxis(width / 2.0f, (height / 2.0f) + 15.0f, statistics.FloorSetStatistics != FloorSetStatistics.Summary ? $"No data on {floorNumber}" : "Change the page to any floor set to view the data.", Color.White, Alignment.Center);

        this.WindowSizeUpdate(width, height, ui.Scale);
        this.CheckForEvents();
    }
}