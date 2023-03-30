using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using static DeepDungeonTracker.DataStatistics;

namespace DeepDungeonTracker;

public sealed class StatisticsWindow : WindowEx, IDisposable
{
    private Data Data { get; }

    private Button ScreenshotButton { get; } = new ScreenshotButton();

    private Button DoubleArrowButtonSummary { get; } = new DoubleArrowButton(false);

    private Button DoubleArrowButtonCurrent { get; } = new DoubleArrowButton(true);

    private Button ArrowButtonPrevious { get; } = new ArrowButton(false);

    private Button ArrowButtonNext { get; } = new ArrowButton(true);

    private Button CloseButton { get; } = new CloseButton();

    private IList<Button> FloorSetSummaryButtons { get; } = new List<Button>();

    private IDictionary<uint, (uint, string)> ClassJobIds { get; }

    public StatisticsWindow(string id, Configuration configuration, Data data) : base(id, configuration, WindowEx.StaticNoBackgroundMoveInputs)
    {
        this.Data = data;
        this.ClassJobIds = new Dictionary<uint, (uint, string)>()
        {
            { 1, ( 0, "GLA")}, {19, ( 0, "PLD")},
            { 2, ( 1, "PGL")}, {20, ( 1, "MNK")},
            { 3, ( 2, "MRD")}, {21, ( 2, "WAR")},
            { 4, ( 3, "LNC")}, {22, ( 3, "DRG")},
            { 5, ( 4, "ARC")}, {23, ( 5, "BRD")},
            { 6, ( 6, "CNJ")}, {24, ( 6, "WHM")},
            { 7, ( 7, "THM")}, {25, ( 7, "BLM")},
            {26, ( 8, "ACN")}, {27, ( 9, "SMN")}, {28, (10, "SCH")},
            {29, (11, "ROG")}, {30, (11, "NIN")},
            {31, (12, "MCH")},
            {32, (13, "DRK")},
            {33, (14, "AST")},
            {34, (15, "SAM")},
            {35, (16, "RDM")},
            {36, (17, "BLU")},
            {37, (18, "GNB")},
            {38, (19, "DNC")},
            {39, (20, "RPR")},
            {40, (21, "SGE")}
        };

        for (int i = 0; i < 20; i++)
            this.FloorSetSummaryButtons.Add(new GenericButton());
    }

    public void Dispose() { }

    public void CheckForEvents()
    {
        var statistics = this.Data.Statistics;

        if (this.DoubleArrowButtonSummary.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            statistics.FloorSetStatisticsSummary();
        }
        else if (this.ArrowButtonPrevious.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            statistics.FloorSetStatisticsPrevious();
        }
        else if (this.ArrowButtonNext.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            statistics.FloorSetStatisticsNext();
        }
        else if (this.DoubleArrowButtonCurrent.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.OnClick);
            statistics.FloorSetStatisticsCurrent();
        }
        else if (this.CloseButton.OnMouseLeftClick())
        {
            this.IsOpen = false;
        }
        else if (this.ScreenshotButton.OnMouseLeftClick())
        {
            this.Data.Audio.PlaySound(SoundIndex.Screenshot);
            var classJobName = this.ClassJobIds.TryGetValue(statistics.ClassJobId, out var classJobId) ? classJobId.Item2 : string.Empty;
            var fileName = $"{classJobName} {statistics.FloorSetStatistics.GetDescription()} {DateTime.Now.ToString("yyyyMMdd HHmmss", CultureInfo.InvariantCulture)}.png".Trim();
            var fontGlobalScale = ImGui.GetIO().FontGlobalScale;
            var size = this.GetSizeScaled() * (fontGlobalScale > 1.0f ? fontGlobalScale : 1.0f);
            var result = ScreenStream.Screenshot(ImGui.GetWindowPos(), size, Directories.Screenshots, fileName);
            Service.ChatGui.Print(result.Item1 ? $"{result.Item2} ({fileName})" : result.Item2);
        }
    }

    public override void OnOpen() => this.Data.Audio.PlaySound(SoundIndex.OnOpenMenu);

    public override void OnClose() => this.Data.Audio.PlaySound(SoundIndex.OnCloseMenu);

    private Vector4 SummarySelectionColor(bool condition = true) => (this.Data.Statistics.FloorSetStatistics == FloorSetStatistics.Summary && this.Data.Statistics.SaveSlotSummary != null && condition) ? this.Configuration.Statistics.SummarySelectionColor : Color.White;

    private static (float, float) AdjustSummaryFloorSetPosition(float leftPanelAdjust, float x, float y, float top, float lineHeight, int firstFloorNumber)
    {
        if (firstFloorNumber == 91)
        {
            x = 350.0f + leftPanelAdjust;
            y = top;
        }
        else
            y += lineHeight * 2.525f;

        return (x, y);
    }

    private void DrawMiscellaneousIcon(float x, float y, float iconSize, IEnumerable<StatisticsItem<Miscellaneous>>? data, bool includeMapTotal)
    {
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Miscellaneous>>())
        {
            var value = (Miscellaneous)(Enum)item.Value;
            if ((!includeMapTotal && value != Miscellaneous.Map) || includeMapTotal)
            {
                if (value == Miscellaneous.RegenPotion)
                {
                    if (this.Data.Statistics.DeepDungeon == DeepDungeon.PalaceOfTheDead || this.Data.Statistics.DeepDungeon == DeepDungeon.HeavenOnHigh)
                        this.Data.UI.DrawMiscellaneous(x, y, value);
                    else if (this.Data.Statistics.DeepDungeon == DeepDungeon.EurekaOrthos)
                        this.Data.UI.DrawPotsherdRegenPotion(x + 4.0f, y + 4.0f, 1);
                }
                else
                    this.Data.UI.DrawMiscellaneous(x, y, value);
            }
            x += iconSize;
        }
    }

    private void DrawMiscellaneousMap(float x, float y, float iconSize, IEnumerable<StatisticsItem<Miscellaneous>>? data, MapData? mapData)
    {
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Miscellaneous>>())
        {
            var value = (Miscellaneous)(Enum)item.Value;
            if (value == Miscellaneous.Map && mapData != null && mapData.FloorType != FloorType.None)
            {
                var scale = mapData.FloorType == FloorType.Normal ? 8.0f : mapData.FloorType == FloorType.HallOfFallacies ? 3.67f : 1.0f;
                for (var j = 0; j < MapData.Length; j++)
                {
                    for (var i = 0; i < MapData.Length; i++)
                    {
                        var size = (44.0f / scale) * ((mapData.FloorType == FloorType.Normal) ? 2.0f : 1.0f);
                        var posX = x + (i * size);
                        var posY = y + (j * size) - 4.0f;
                        var id = mapData.GetId(i, j);

                        if (id == null)
                            continue;

                        if (mapData.FloorType == FloorType.Normal)
                            this.Data.UI.DrawMapNormal(posX, posY, id.Value);
                        else if (mapData.FloorType == FloorType.HallOfFallacies)
                            this.Data.UI.DrawMapHallOfFallacies(posX, posY, id.Value);
                    }
                }
            }
            x += iconSize;
        }
    }

    private void DrawMiscellaneousText(float x, float y, float iconSize, IEnumerable<StatisticsItem<Miscellaneous>>? data, bool includeMapTotal)
    {
        var offset = 0.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Miscellaneous>>())
        {
            var value = (Miscellaneous)(Enum)item.Value;
            if ((!includeMapTotal && value != Miscellaneous.Map) || includeMapTotal)
            {
                var total = item.Total;
                if (total > 1)
                    this.Data.UI.DrawTextAxis(x + offset + iconSize, y + offset + iconSize, total.ToString(CultureInfo.InvariantCulture), this.SummarySelectionColor(), Alignment.Right);
            }
            x += iconSize;
        }
    }

    private void DrawCofferIcon(float x, float y, float iconSize, IEnumerable<StatisticsItem<Coffer>>? data)
    {
        var offset = 4.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Coffer>>())
        {
            var value = (Coffer)(Enum)item.Value;

            if (value == Coffer.Potsherd)
            {
                if (this.Data.Statistics.DeepDungeon == DeepDungeon.PalaceOfTheDead)
                    this.Data.UI.DrawCoffer(x + offset, y + offset, (Coffer)(Enum)item.Value);
                else if (this.Data.Statistics.DeepDungeon == DeepDungeon.HeavenOnHigh)
                    this.Data.UI.DrawPotsherdRegenPotion(x + offset, y + offset, 0);
                else if (this.Data.Statistics.DeepDungeon == DeepDungeon.EurekaOrthos)
                    this.Data.UI.DrawPotsherdRegenPotion(x + offset, y + offset, 2);
            }
            else
                this.Data.UI.DrawCoffer(x + offset, y + offset, (Coffer)(Enum)item.Value);

            x += iconSize;
        }
    }

    private void DrawCofferText(float x, float y, float iconSize, IEnumerable<StatisticsItem<Coffer>>? data)
    {
        var offset = 0.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Coffer>>())
        {
            var total = item.Total;
            if (total > 1)
                this.Data.UI.DrawTextAxis(x + offset + iconSize, y + offset + iconSize, total.ToString(CultureInfo.InvariantCulture), this.SummarySelectionColor(), Alignment.Right);
            x += iconSize;
        }
    }

    private void DrawPomanderIcon(float x, float y, float iconSize, IEnumerable<StatisticsItem<Pomander>>? data)
    {
        var offset = 4.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Pomander>>())
        {
            this.Data.UI.DrawPomander(x + offset, y + offset, (Pomander)(Enum)item.Value);
            x += iconSize;
        }
    }

    private void DrawPomanderText(float x, float y, float iconSize, IEnumerable<StatisticsItem<Pomander>>? data)
    {
        var offset = 0.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Pomander>>())
        {
            var total = item.Total;
            if (total > 1)
                this.Data.UI.DrawTextAxis(x + offset + iconSize, y + offset + iconSize, total.ToString(CultureInfo.InvariantCulture), this.SummarySelectionColor(), Alignment.Right);
            x += iconSize;
        }
    }

    private void DrawEnchantmentIcon(float x, float y, float iconSize, IEnumerable<StatisticsItem<Enchantment>>? data, bool isEnchantmentSerenized)
    {
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Enchantment>>())
        {
            this.Data.UI.DrawEnchantment(x, y, (Enchantment)(Enum)item.Value, isEnchantmentSerenized);
            x += iconSize;
        }
    }

    private void DrawEnchantmentText(float x, float y, float iconSize, IEnumerable<StatisticsItem<Enchantment>>? data)
    {
        var offset = 0.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Enchantment>>())
        {
            var total = item.Total;
            if (total > 1)
                this.Data.UI.DrawTextAxis(x + offset + iconSize, y + offset + iconSize, total.ToString(CultureInfo.InvariantCulture), this.SummarySelectionColor(), Alignment.Right);
            x += iconSize;
        }
    }

    private void DrawTrapIcon(float x, float y, float iconSize, IEnumerable<StatisticsItem<Trap>>? data)
    {
        var offset = 8.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Trap>>())
        {
            this.Data.UI.DrawTrap(x + offset, y + offset, (Trap)(Enum)item.Value);
            x += iconSize;
        }
    }

    private void DrawTrapText(float x, float y, float iconSize, IEnumerable<StatisticsItem<Trap>>? data)
    {
        var offset = 0.0f;
        foreach (var item in data ?? Enumerable.Empty<StatisticsItem<Trap>>())
        {
            var total = item.Total;
            if (total > 1)
                this.Data.UI.DrawTextAxis(x + offset + iconSize, y + offset + iconSize, total.ToString(CultureInfo.InvariantCulture), this.SummarySelectionColor(), Alignment.Right);
            x += iconSize;
        }
    }

    private void DrawGotUsedText(float x, float y, float iconSize, IEnumerable<StatisticsItem<Coffer>>? cofferData, IEnumerable<StatisticsItem<Pomander>>? pomanderData)
    {
        var color = new Vector4(1.0f, 1.0f, 1.0f, 0.7f);

        var offset = 4.0f;
        var adjustY = -4.0f;
        if (cofferData?.Count() > 0)
            this.Data.UI.DrawTextMiedingerMid(x + offset, y + offset + adjustY, "Got", color);

        if (pomanderData?.Count() > 0)
            this.Data.UI.DrawTextMiedingerMid(x + offset, y + offset + iconSize + adjustY, "Used", color);
    }

    private void DrawFloorText(float x, float y, string floorText, TimeSpan totalTime, TimeSpan? previousTime, int totalScore, int? previousScore, bool forceShowHours = false, bool isTimeBonusMissScore = false)
    {
        var config = this.Configuration.Statistics;
        var ui = this.Data.UI;
        isTimeBonusMissScore = isTimeBonusMissScore && this.Data.Common.Score?.TotalScore != 0;
        var lineHeight = 28.0f;

        void DrawTotalScore()
        {
            if (totalScore != 0)
            {
                ui.DrawTextAxis(x, y, $"{totalScore.ToString("N0", CultureInfo.InvariantCulture)}", totalScore > 0 ? config!.ScoreColor : totalScore < 0 ? Color.Red : Color.White);
                if (previousScore != null && previousScore.Value != totalScore && previousScore != 0)
                {
                    var previousScoreText = $"{(previousScore > 0 ? "+" : string.Empty)}{previousScore.Value.ToString("N0", CultureInfo.InvariantCulture)}";
                    ui.DrawTextAxis(x, y + lineHeight, previousScoreText, previousScore > 0 ? config!.ScoreColor : previousScore < 0 ? Color.Red : Color.White);
                }
            }
        }

        void DrawTimeBonusMissScore()
        {
            var timeBonusMissScore = totalScore + (101 * 150);
            var timeBonusMissScoreText = $"{timeBonusMissScore.ToString("N0", CultureInfo.InvariantCulture)}";
            ui.DrawTextAxis(x, y, timeBonusMissScoreText, timeBonusMissScore > 0 ? config!.ScoreColor : timeBonusMissScore < 0 ? Color.Red : Color.White);
            x += ui!.GetAxisTextSize(timeBonusMissScoreText).X;
        }

        ui.DrawTextAxis(x, y, floorText, this.SummarySelectionColor(this.Data.Statistics.FloorSetTextSummary == floorText));

        var space = "   ";
        floorText += space;
        var totalTimeText = (totalTime.Hours > 0 || forceShowHours) ? $"{totalTime}" : $"{totalTime:mm\\:ss}";
        totalTimeText += space;

        if (totalTime != default)
        {
            x += ui.GetAxisTextSize(floorText).X;
            ui.DrawTextAxis(x, y, totalTimeText, config.FloorTimeColor);
            if (previousTime != null && previousTime.Value != totalTime && previousTime.Value != default)
            {
                var previousTimeText = "+";
                previousTimeText += previousTime.Value.Hours > 0 ? $"{previousTime}" : $"{previousTime:mm\\:ss}";
                ui.DrawTextAxis(x, y + lineHeight, previousTimeText, config.FloorTimeColor);
            }
            x += ui!.GetAxisTextSize(totalTimeText).X;
        }

        var timeBonusX = 0.0f;
        if (isTimeBonusMissScore)
        {
            DrawTimeBonusMissScore();
            timeBonusX = x;
            x += 48.0f;
            DrawTotalScore();
        }
        else
            DrawTotalScore();

        if (isTimeBonusMissScore)
            ui.DrawMiscellaneous(timeBonusX, y - 15.0f, Miscellaneous.TimeBonus);
    }

    private void DrawLastFloorAndTotal(float lastFloorX, float totalX, float y, bool forceShowHours, bool isTimeBonusMissScore)
    {
        var statistics = this.Data.Statistics;
        this.DrawFloorText(lastFloorX, y - 20.0f, "Last Floor:", statistics!.TimeLastFloor, null, statistics.ScoreLastFloor, null, forceShowHours, isTimeBonusMissScore);
        this.DrawFloorText(totalX, y - 20.0f, "Total:", statistics!.TimeTotal, null, statistics.ScoreTotal, null, forceShowHours, false);
    }

    private void DrawSummaryPageTexts(float leftPanelAdjust, float left, float top, float iconSize)
    {
        var statistics = this.Data.Statistics;
        var floorSets = statistics.SaveSlot?.FloorSets ?? Enumerable.Empty<FloorSet>();
        var ui = this.Data.UI;

        var x = left + leftPanelAdjust;
        var y = top;

        var totalTime = 0L;
        var totalScore = 0;
        var totalKills = 0;
        var lineHeight = 28.0f;
        foreach (var item in floorSets)
        {
            var firstFloorNumber = item.FirstFloor()?.Number ?? 0;
            totalTime += item.Time().Ticks;
            totalScore += item.Score();
            totalKills += item.Kills();
            this.DrawFloorText(x, y, $"{firstFloorNumber:D3}-{firstFloorNumber + 9:D3}:", new TimeSpan(totalTime), item.Time(), totalScore, item.Score(), true);

            var kills = item.Kills();
            if (kills != totalKills && kills > 0)
                this.Data.UI.DrawTextAxis(x + iconSize - 16.0f, y + 35.0f, $"+{kills.ToString(CultureInfo.InvariantCulture)}", Color.Yellow, Alignment.Left);
            this.Data.UI.DrawTextAxis(x + iconSize - 16.0f, y + 21.0f + (kills == totalKills || kills == 0 ? 8.0f : 0.0f), totalKills.ToString(CultureInfo.InvariantCulture), Color.White, Alignment.Left);

            (x, y) = StatisticsWindow.AdjustSummaryFloorSetPosition(leftPanelAdjust, x, y, top, lineHeight, firstFloorNumber);
        }

        x = left;
        y = top;

        foreach (var item in this.FloorSetSummaryButtons)
            item.Show = false;

        var buttonIndex = 0;
        foreach (var item in floorSets)
        {
            var firstFloorNumber = item.FirstFloor()?.Number ?? 0;
            var floorSetText = $"{firstFloorNumber:D3}-{firstFloorNumber + 9:D3}:";

            var button = this.FloorSetSummaryButtons[buttonIndex];

            button.Show = true;
            button.Position = new Vector2(x + leftPanelAdjust, y);
            button.Size = new(240.0f, 48.0f);
            button.Draw(ui, this.Data.Audio);

            if (button.OnMouseLeftClick())
            {
                this.Data.Audio.PlaySound(SoundIndex.OnClick);
                statistics.FloorSetStatisticsSummarySelection(firstFloorNumber, floorSetText, this.Configuration.Score.ScoreCalculationType);
            }

            buttonIndex++;

            (x, y) = StatisticsWindow.AdjustSummaryFloorSetPosition(0.0f, x, y, top, lineHeight, firstFloorNumber);
        }

        x = 1085.0f;
        y = top;
        var score = statistics.ScoreSummary ?? this.Data.Common.Score;
        if (score != null)
        {
            var color = this.SummarySelectionColor();

            ui.DrawTextAxis(x, y, $"{(score.IsDutyComplete ? "Duty Complete" : "Duty Failed")} ({score.BaseScore})", color); y += lineHeight * 2.0f;
            ui.DrawTextAxis(x, y, $"Level: {score.CurrentLevel} (+{score.AetherpoolArm}/+{score.AetherpoolArmor})", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Floor: {score.StartingFloorNumber} to {score.CurrentFloorNumber} ({score.TotalReachedFloors})", color); y += lineHeight * 2.0f;
            ui.DrawTextAxis(x, y, $"Character: {score.CharacterScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Floors: {score.FloorScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Maps: {score.MapScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Coffers: {score.CofferScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;

            if (statistics?.SaveSlot?.DeepDungeon == DeepDungeon.PalaceOfTheDead)
            {
                ui.DrawTextAxis(x, y, $"NPCs: {score.NPCScore.ToString("N0", CultureInfo.InvariantCulture)}", color);
                y += lineHeight;
            }

            if (statistics?.SaveSlot?.DeepDungeon == DeepDungeon.EurekaOrthos)
            {
                ui.DrawTextAxis(x, y, $"Dread Beasts: {score.DreadBeastScore.ToString("N0", CultureInfo.InvariantCulture)}", color);
                y += lineHeight;
            }

            ui.DrawTextAxis(x, y, $"Mimicgoras: {score.MimicgoraScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Enchantments: {score.EnchantmentScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Traps: {score.TrapScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Time Bonuses: {score.TimeBonusScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Deaths: {score.DeathScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight * 2.0f;
            ui.DrawTextAxis(x, y, $"Non-Kills: {score.NonKillScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight;
            ui.DrawTextAxis(x, y, $"Kills: {score.KillScore.ToString("N0", CultureInfo.InvariantCulture)}", color); y += lineHeight * 2.0f;
            ui.DrawTextAxis(x, y, $"Total: {score.TotalScore.ToString("N0", CultureInfo.InvariantCulture)}", color);
        }
    }

    private void DrawFloorSetKillIcons(float leftPanelAdjust, float top, float x)
    {
        var statistics = this.Data.Statistics;
        var floorSets = statistics.SaveSlot?.FloorSets;

        var lineHeight = 28.0f;
        var y = top;

        foreach (var item in floorSets ?? Enumerable.Empty<FloorSet>())
        {
            var firstFloorNumber = item.FirstFloor()?.Number ?? 0;
            this.Data.UI.DrawMiscellaneous(x - 10.0f + leftPanelAdjust, y + 10.0f, Miscellaneous.Enemy);

            (x, y) = StatisticsWindow.AdjustSummaryFloorSetPosition(0.0f, x, y, top, lineHeight, firstFloorNumber);
        }
    }

    private void SummaryPage(float leftPanelAdjust, float left, float top, float iconSize, float floorWidth, float floorHeight)
    {
        var statistics = this.Data.Statistics;

        var x = left;
        var y = top + 20.0f;
        var x2 = x + (floorWidth * 2.0f) + leftPanelAdjust;
        var y3 = y + (floorHeight * 3.0f);
        var iconSize2 = iconSize * 2.0f;
        var iconSize3 = iconSize * 3.0f;

        this.DrawFloorSetKillIcons(leftPanelAdjust, top, x);
        this.DrawMiscellaneousIcon(x2, y3, iconSize, statistics?.MiscellaneousLastFloor, false);
        this.DrawMiscellaneousIcon(x, y3, iconSize, statistics?.MiscellaneousTotal, true);
        this.DrawCofferIcon(x, y3 + iconSize, iconSize, statistics?.CoffersTotal);
        this.DrawPomanderIcon(x2, y3 + iconSize, iconSize, statistics?.PomandersLastFloor);
        this.DrawPomanderIcon(x, y3 + iconSize2, iconSize, statistics?.PomandersTotal);
        this.DrawEnchantmentIcon(x, y3 + iconSize3, iconSize, statistics?.EnchantmentsTotal, false);
        this.DrawTrapIcon(x + ((statistics?.EnchantmentsTotal?.Count() ?? 0) * iconSize), y3 + iconSize3, iconSize, statistics?.TrapsTotal);

        this.DrawSummaryPageTexts(leftPanelAdjust, left, top, iconSize);
        this.DrawLastFloorAndTotal(x2, x, y3, true, false);

        this.DrawMiscellaneousText(x2, y3, iconSize, statistics?.MiscellaneousLastFloor, false);
        this.DrawMiscellaneousText(x, y3, iconSize, statistics?.MiscellaneousTotal, true);
        this.DrawCofferText(x, y3 + iconSize, iconSize, statistics?.CoffersTotal);
        this.DrawPomanderText(x2, y3 + iconSize, iconSize, statistics?.PomandersLastFloor);
        this.DrawPomanderText(x, y3 + iconSize2, iconSize, statistics?.PomandersTotal);
        this.DrawEnchantmentText(x, y3 + iconSize3, iconSize, statistics?.EnchantmentsTotal);
        this.DrawTrapText(x + ((statistics?.EnchantmentsTotal?.Count() ?? 0) * iconSize), y3 + iconSize3, iconSize, statistics?.TrapsTotal);

        this.DrawGotUsedText(x, y3 + iconSize, iconSize, statistics?.CoffersTotal, statistics?.PomandersTotal);
    }

    private void FloorSetPage(float leftPanelAdjust, float left, float top, float iconSize, float floorWidth, float floorHeight, float width)
    {
        static void FloorLoop(Action<Floor, int, float, float> action, IReadOnlyList<Floor>? floors, float left, float x, float y, float iconSize, float floorWidth, float floorHeight)
        {
            for (var i = 0; i < 9; i++)
            {
                var floor = i < floors?.Count ? floors[i] : null;
                if (floor == null)
                    continue;
                action?.Invoke(floor, i, x, y);
                (x, y) = (i % 3 != 2) ? (x + floorWidth, y) : (left, y + floorHeight);
            }
        }

        static void CheckForTimeBonusMissScore(ref TimeSpan timeBonusMissScoreTotal, ref bool isTimeBonusMissScore, TimeSpan floorTime)
        {
            if (isTimeBonusMissScore)
            {
                timeBonusMissScoreTotal = TimeSpan.MinValue;
                isTimeBonusMissScore = false;
            }

            if (timeBonusMissScoreTotal != TimeSpan.MinValue)
            {
                timeBonusMissScoreTotal += floorTime;
                if (timeBonusMissScoreTotal > new TimeSpan(0, 30, 0))
                    isTimeBonusMissScore = true;
            }
        }

        var statistics = this.Data.Statistics;
        var floors = statistics.FloorSet?.Floors;

        left += leftPanelAdjust;
        var x = left;
        var y = top + 20.0f;
        var x2 = x + (floorWidth * 2.0f);
        var y3 = y + (floorHeight * 3.0f);
        var iconSize2 = iconSize * 2.0f;
        var iconSize3 = iconSize * 3.0f;

        for (var i = 1; i <= 2; i++)
            this.Data.UI.DrawDivisorHorizontal(leftPanelAdjust + 3.0f, 34.0f + (floorHeight * i), width - 185.0f);

        for (var j = 0; j < 3; j++)
            for (var i = 1; i <= 2; i++)
                this.Data.UI.DrawDivisorVertical(4.0f + (floorWidth * i) + leftPanelAdjust, 37.0f + (floorHeight * j), floorHeight + 1.0f);

        FloorLoop((floor, index, x, y) => { this.DrawMiscellaneousIcon(x, y, iconSize, statistics?.MiscellaneousByFloor?.ElementAtOrDefault(index), false); }, floors, left, x, y, iconSize, floorWidth, floorHeight);
        this.DrawMiscellaneousIcon(x2, y3, iconSize, statistics?.MiscellaneousLastFloor, false);
        this.DrawMiscellaneousIcon(x - leftPanelAdjust, y3, iconSize, statistics?.MiscellaneousTotal, true);
        FloorLoop((floor, index, x, y) => { this.DrawMiscellaneousMap(x, y, iconSize, statistics?.MiscellaneousByFloor?.ElementAtOrDefault(index), floor?.MapData); }, floors, left, x, y, iconSize, floorWidth, floorHeight);
        FloorLoop((floor, index, x, y) => { this.DrawCofferIcon(x, y, iconSize, statistics?.CoffersByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize, iconSize, floorWidth, floorHeight);
        this.DrawCofferIcon(x - leftPanelAdjust, y3 + iconSize, iconSize, statistics?.CoffersTotal);
        FloorLoop((floor, index, x, y) => { this.DrawPomanderIcon(x, y, iconSize, statistics?.PomandersByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize2, iconSize, floorWidth, floorHeight);
        this.DrawPomanderIcon(x2, y3 + iconSize, iconSize, statistics?.PomandersLastFloor);
        this.DrawPomanderIcon(x - leftPanelAdjust, y3 + iconSize2, iconSize, statistics?.PomandersTotal);
        FloorLoop((floor, index, x, y) => { this.DrawEnchantmentIcon(x, y, iconSize, statistics?.EnchantmentsByFloor?.ElementAtOrDefault(index), floor?.EnchantmentsSerenized.Count > 0); }, floors, left, x, y + iconSize3, iconSize, floorWidth, floorHeight);
        this.DrawEnchantmentIcon(x - leftPanelAdjust, y3 + iconSize3, iconSize, statistics?.EnchantmentsTotal, false);
        FloorLoop((floor, index, x, y) => { this.DrawTrapIcon(x + ((statistics?.EnchantmentsByFloor?.ElementAtOrDefault(index)?.Count() ?? 0) * iconSize), y, iconSize, statistics?.TrapsByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize3, iconSize, floorWidth, floorHeight);
        this.DrawTrapIcon(x + ((statistics?.EnchantmentsTotal?.Count() ?? 0) * iconSize) - leftPanelAdjust, y3 + iconSize3, iconSize, statistics?.TrapsTotal);

        TimeSpan timeBonusMissScoreTotal = TimeSpan.Zero;
        bool isTimeBonusMissScore = false;

        FloorLoop((floor, index, x, y) =>
        {
            CheckForTimeBonusMissScore(ref timeBonusMissScoreTotal, ref isTimeBonusMissScore, floor.Time);
            this.DrawFloorText(x, y, $"Floor {floor.Number}:", floor.Time, null, floor.Score, null, false, isTimeBonusMissScore);
        }, floors, left, x, y - 20.0f, iconSize, floorWidth, floorHeight);

        CheckForTimeBonusMissScore(ref timeBonusMissScoreTotal, ref isTimeBonusMissScore, statistics?.FloorSet?.LastFloor()?.Time ?? default);
        this.DrawLastFloorAndTotal(x2, x - leftPanelAdjust, y3, false, isTimeBonusMissScore);

        FloorLoop((floor, index, x, y) => { this.DrawMiscellaneousText(x, y, iconSize, statistics?.MiscellaneousByFloor?.ElementAtOrDefault(index), false); }, floors, left, x, y, iconSize, floorWidth, floorHeight);
        this.DrawMiscellaneousText(x2, y3, iconSize, statistics?.MiscellaneousLastFloor, false);
        this.DrawMiscellaneousText(x - leftPanelAdjust, y3, iconSize, statistics?.MiscellaneousTotal, true);
        FloorLoop((floor, index, x, y) => { this.DrawCofferText(x, y, iconSize, statistics?.CoffersByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize, iconSize, floorWidth, floorHeight);
        this.DrawCofferText(x - leftPanelAdjust, y3 + iconSize, iconSize, statistics?.CoffersTotal);
        FloorLoop((floor, index, x, y) => { this.DrawPomanderText(x, y, iconSize, statistics?.PomandersByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize2, iconSize, floorWidth, floorHeight);
        this.DrawPomanderText(x2, y3 + iconSize, iconSize, statistics?.PomandersLastFloor);
        this.DrawPomanderText(x - leftPanelAdjust, y3 + iconSize2, iconSize, statistics?.PomandersTotal);
        FloorLoop((floor, index, x, y) => { this.DrawEnchantmentText(x, y, iconSize, statistics?.EnchantmentsByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize3, iconSize, floorWidth, floorHeight);
        this.DrawEnchantmentText(x - leftPanelAdjust, y3 + iconSize3, iconSize, statistics?.EnchantmentsTotal);
        FloorLoop((floor, index, x, y) => { this.DrawTrapText(x + ((statistics?.EnchantmentsByFloor?.ElementAtOrDefault(index)?.Count() ?? 0) * iconSize), y, iconSize, statistics?.TrapsByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize3, iconSize, floorWidth, floorHeight);
        this.DrawTrapText(x + ((statistics?.EnchantmentsTotal?.Count() ?? 0) * iconSize) - leftPanelAdjust, y3 + iconSize3, iconSize, statistics?.TrapsTotal);

        FloorLoop((floor, index, x, y) => { this.DrawGotUsedText(x, y, iconSize, statistics?.CoffersByFloor?.ElementAtOrDefault(index), statistics?.PomandersByFloor?.ElementAtOrDefault(index)); }, floors, left, x, y + iconSize, iconSize, floorWidth, floorHeight);
        this.DrawGotUsedText(x - leftPanelAdjust, y3 + iconSize, iconSize, statistics?.CoffersTotal, statistics?.PomandersTotal);
    }

    public override void Draw()
    {
        var statistics = this.Data.Statistics;
        var ui = this.Data.UI;
        var config = this.Configuration.Statistics;
        ui.Scale = config.Scale;
        var audio = this.Data.Audio;

        var leftPanelAdjust = 170.0f;
        var width = 1359.0f + leftPanelAdjust;
        var height = 989.0f;

        var floorSet = statistics.FloorSet;
        var floorSets = statistics.FloorSets;

        ui.DrawBackground(width, height, (!config.SolidBackground && this.IsFocused) || config.SolidBackground);

        if (this.ClassJobIds.TryGetValue(statistics.ClassJobId, out var classJobId))
            ui.DrawJob(leftPanelAdjust / 2.0f, 100.0f, classJobId.Item1);
        else
            ui.DrawJob(leftPanelAdjust / 2.0f, 100.0f, 17);

        if (statistics.DeepDungeon != DeepDungeon.None)
            ui.DrawDeepDungeon(width / 2.0f, 20.0f, statistics.DeepDungeon);

        this.ScreenshotButton.Position = new Vector2(15.0f, 160.0f);
        this.DoubleArrowButtonSummary.Position = new(15.0f, 200.0f);
        this.DoubleArrowButtonCurrent.Position = new(129.0f, 200.0f);
        this.ArrowButtonPrevious.Position = new(51.0f, 200.0f);
        this.ArrowButtonNext.Position = new(91.0f, 200.0f);
        this.CloseButton.Position = new(width - 35.0f, 7.0f);
        this.ScreenshotButton.Draw(ui, audio);
        this.DoubleArrowButtonSummary.Draw(ui, audio);
        this.DoubleArrowButtonCurrent.Draw(ui, audio);
        this.ArrowButtonPrevious.Draw(ui, audio);
        this.ArrowButtonNext.Draw(ui, audio);
        this.CloseButton.Draw(ui, audio);

        ui.DrawDivisorHorizontal(14.0f, 34.0f, width - 26.0f);

        if (floorSet != null || floorSets?.Count() > 0)
        {
            var left = 15.0f;
            var top = 50.0f;
            var iconSize = 48.0f;
            var floorWidth = 450.0f;
            var floorHeight = 237.0f;

            ui.DrawDivisorHorizontal(14.0f, 745.0f, width - 26.0f);
            ui.DrawDivisorVertical(leftPanelAdjust, 37.0f, height - 277.0f);
            ui.DrawDivisorVertical(4.0f + (floorWidth * 2) + leftPanelAdjust, 37.0f + (floorHeight * 3), floorHeight - 8.0f);

            if (statistics.FloorSetStatistics == FloorSetStatistics.Summary)
            {
                ui.DrawDivisorVertical(4.0f + (floorWidth * 2) + leftPanelAdjust, 37.0f, height - 277.0f);
                this.SummaryPage(leftPanelAdjust, left, top, iconSize, floorWidth, floorHeight);
            }
            else
                this.FloorSetPage(leftPanelAdjust, left, top, iconSize, floorWidth, floorHeight, width);
        }
        else
        {
            ui.DrawDivisorVertical(leftPanelAdjust, 37.0f, height - 49.0f);
            ui.DrawTextAxis(width / 2.0f, (height / 2.0f) + 15.0f, $"No data on {statistics.FloorSetStatistics.GetDescription()}", Color.White, Alignment.Center);
        }

        ui.DrawTextTrumpGothic(15.0f, 8.0f, "Statistics", new(0.8197f, 0.8197f, 0.8197f, 1.0f), Alignment.Left);

        this.WindowSizeUpdate(width, height, ui.Scale);
        this.CheckForEvents();
    }
}