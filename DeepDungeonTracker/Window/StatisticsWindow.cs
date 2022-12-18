using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepDungeonTracker
{
    public sealed class StatisticsWindow : WindowEx, IDisposable
    {
        private Data Data { get; }

        private Button ArrowButtonPrevious { get; set; } = new ArrowButton(false);

        private Button ArrowButtonNext { get; set; } = new ArrowButton(true);

        private Button CloseButton { get; set; } = new CloseButton();

        private IDictionary<uint, uint> ClassJobIds { get; set; }

        public StatisticsWindow(string id, Configuration configuration, Data data) : base(id, configuration, WindowEx.StaticNoBackgroundMoveInputs)
        {
            this.Data = data;
            this.IsOpen = true;
            this.ClassJobIds = new Dictionary<uint, uint>()
            {
                {  1 /*GLA*/ ,0}, { 19 /*PLD*/ ,0},
                {  2 /*PGL*/ ,1}, { 20 /*MNK*/ ,1},
                {  3 /*MRD*/ ,2}, { 21 /*WAR*/ ,2},
                {  4 /*LNC*/ ,3}, { 22 /*DRG*/ ,3},
                {  5 /*ARC*/ ,4}, { 23 /*BRD*/ ,5},
                {  6 /*CNJ*/ ,6}, { 24 /*WHM*/ ,6},
                {  7 /*THM*/ ,7}, { 25 /*BLM*/ ,7},
                { 26 /*ACN*/ ,8}, { 27 /*SMN*/ ,9}, { 28 /*SCH*/ ,10},
                { 29 /*ROG*/ ,11},{ 30 /*NIN*/ ,11},
                { 31 /*MCH*/ ,12},
                { 32 /*DRK*/ ,13},
                { 33 /*AST*/ ,14},
                { 34 /*SAM*/ ,15},
                { 35 /*RDM*/ ,16},
                { 36 /*BLU*/ ,17},
                { 37 /*GNB*/ ,18},
                { 38 /*DNC*/ ,19},
                { 39 /*RPR*/ ,20},
                { 40 /*SGE*/ ,21}
            };
        }

        public void Dispose() { }

        public override void PreOpenCheck()
        {
            var statistics = this.Data.Statistics;

            if (this.ArrowButtonPrevious.OnMouseLeftClick())
                statistics.FloorSetStatisticsPrevious();

            else if (this.ArrowButtonNext.OnMouseLeftClick())
                statistics.FloorSetStatisticsNext();

            else if (this.CloseButton.OnMouseLeftClick())
                statistics.Open = false;

            this.IsOpen = statistics.Open;
        }

        public override void Draw()
        {
            var config = this.Configuration.Statistics;
            var statistics = this.Data.Statistics;
            var ui = this.Data.UI;
            ui.Scale = config.Scale;
            var left = 15.0f;
            var top = 50.0f;
            var width = 1359.0f;
            var height = 989.0f;
            var floorWidth = 450.0f;
            var floorHeight = 189.0f + 48.0f;
            var floorSet = statistics.FloorSet;
            var floorSets = statistics.FloorSets;
            var noData = (floorSet == null && floorSets == null);

            ui.DrawBackground(width, height, (!this.Configuration.General.SolidBackgroundWindow && this.IsFocused) || this.Configuration.General.SolidBackgroundWindow);

            if (this.ClassJobIds.TryGetValue(statistics.ClassJobId, out var classJobId))
                ui.DrawJob(12.0f, 10.0f, classJobId);

            ui.DrawTextMiedingerMediumW00(width / 2.0f, 20.0f, "Statistics", Color.White, Alignment.Center);
            ui.DrawDivisorHorizontal(45.0f, 34.0f, width - 57.0f);

            this.ArrowButtonPrevious.Position = new((width / 2.0f) - 108.0f, 7.0f);
            this.ArrowButtonNext.Position = new((width / 2.0f) + 72.0f, 7.0f);
            this.CloseButton.Position = new(width - 35.0f, 7.0f);
            this.ArrowButtonPrevious.Draw(ui);
            this.ArrowButtonNext.Draw(ui);
            this.CloseButton.Draw(ui);

            if (!noData)
            {
                var x = left;
                var y = top;
                var miscellaneousOffset = 0.0f;
                var cofferOffset = 4.0f;
                var pomanderOffset = cofferOffset;
                var enchantmentOffset = 0.0f;
                var trapOffset = 8.0f;
                var textOffset = -4.0f;
                var iconSize = 48.0f;
                var isSummary = statistics.FloorSetStatistics == FloorSetStatistics.Summary;
                float baseX;
                float baseY;
                if (!isSummary)
                {
                    var horizontalElements = 3;
                    var floors = floorSet?.Floors;
                    for (var i = 0; i < 9; i++)
                    {
                        baseX = x;
                        baseY = y;
                        var floor = i < floors?.Count ? floors[i] : new(0);

                        if (floor.Number > 0)
                        {
                            this.DrawFloorText(x, y, $"Floor {floor.Number}:", floor.Time, null, floor.Score, null);
                            this.DrawIcon(ref x, ref y, baseX, y, 0.0f, 20.0f, miscellaneousOffset, miscellaneousOffset, textOffset, textOffset, iconSize, i < statistics.MiscellaneousByFloor?.Count ? statistics.MiscellaneousByFloor[i] : default, floor.MapData);
                            this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, cofferOffset, cofferOffset, textOffset, textOffset, iconSize, i < statistics.CoffersByFloor?.Count ? statistics.CoffersByFloor[i] : default);
                            this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, pomanderOffset, pomanderOffset, textOffset, textOffset, iconSize, i < statistics.PomandersByFloor?.Count ? statistics.PomandersByFloor[i] : default);
                            this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, enchantmentOffset, enchantmentOffset, textOffset, textOffset, iconSize, i < statistics.EnchantmentsByFloor?.Count ? statistics.EnchantmentsByFloor[i] : default);
                            this.DrawIcon(ref x, ref y, x, y, 0.0f, 0.0f, trapOffset, trapOffset, textOffset, textOffset, iconSize, i < statistics.TrapsByFloor?.Count ? statistics.TrapsByFloor[i] : default);


                        }

                        if (!new int[] { 2, 5, 8 }.Contains(i))
                            ui.DrawDivisorVertical(baseX + floorWidth - 11.0f, baseY - 13.0f, floorHeight + 1.0f);

                        x = baseX + floorWidth;
                        y = baseY;
                        if (i % horizontalElements == horizontalElements - 1)
                        {
                            x = left;
                            y += floorHeight;
                        }

                        if (i % horizontalElements == 0)
                            ui.DrawDivisorHorizontal(14.0f, baseY + floorHeight - 16.0f, width - 26.0f);
                    }
                }
                else
                {
                    x = left;
                    var totalTime = 0L;
                    var totalScore = 0;
                    var lineHeight = 28.0f;
                    foreach (var item in floorSets ?? Enumerable.Empty<FloorSet>())
                    {
                        var firstFloorNumber = item.FirstFloor()?.Number ?? 0;
                        totalTime += item.Time().Ticks;
                        totalScore += item.Score();
                        this.DrawFloorText(x, y, $"{firstFloorNumber:D3}-{firstFloorNumber + 9:D3}:", new TimeSpan(totalTime), item.Time(), totalScore, item.Score(), true);
                        if (firstFloorNumber == 91)
                        {
                            x = 350.0f;
                            y = top;
                        }
                        else
                            y += lineHeight * 2.55f;
                    }

                    x = floorSets?.Count() > 10 ? 700.0f : 350.0f;
                    y = top;
                    var score = this.Data.Common.Score;
                    if (score != null)
                    {
                        ui.DrawTextAxisLatinPro(x, y, $"[{(score.IsDutyComplete ? "Duty Complete" : "Duty Failed")}] ({score.BaseScore})", Color.White); y += lineHeight * 2.0f;
                        ui.DrawTextAxisLatinPro(x, y, $"Level: {score.CurrentLevel} (+{score.AetherpoolArm}/+{score.AetherpoolArmor})", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Floor: {score.StartingFloorNumber}->{score.CurrentFloorNumber} ({score.TotalReachedFloors})", Color.White); y += lineHeight * 2.0f;
                        ui.DrawTextAxisLatinPro(x, y, $"Character: {score.CharacterScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Floors: {score.FloorScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Maps: {score.MapScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Coffers: {score.CofferScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"NPCs: {score.NPCScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Mimicgoras: {score.MimicgoraScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Enchantments: {score.EnchantmentScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Traps: {score.TrapScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Time Bonuses: {score.TimeBonusScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Deaths: {score.DeathScore:N0}", Color.White); y += lineHeight * 2.0f;
                        ui.DrawTextAxisLatinPro(x, y, $"Non-Kills: {score.NonKillScore:N0}", Color.White); y += lineHeight;
                        ui.DrawTextAxisLatinPro(x, y, $"Kills: {score.KillScore:N0}", Color.White); y += lineHeight * 2.0f;
                        ui.DrawTextAxisLatinPro(x, y, $"Total: {score.TotalScore:N0}", Color.White);
                    }

                    y = 793.0f - 48;
                    ui.DrawDivisorHorizontal(14.0f, 793.0f - 48, width - 26.0f);
                    y += 16.0f;
                }

                x = left;

                this.DrawFloorText(x, y, $"Total:", statistics.TimeTotal, null, statistics.ScoreTotal, null, isSummary);

                baseX = x;
                baseY = y;
                this.DrawIcon(ref x, ref y, baseX, y, 0.0f, 20.0f, miscellaneousOffset, miscellaneousOffset, textOffset, textOffset, iconSize, statistics.MiscellaneousTotal);
                this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, cofferOffset, cofferOffset, textOffset, textOffset, iconSize, statistics.CoffersTotal);
                this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, pomanderOffset, pomanderOffset, textOffset, textOffset, iconSize, statistics.PomandersTotal);
                this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, enchantmentOffset, enchantmentOffset, textOffset, textOffset, iconSize, statistics.EnchantmentsTotal);
                this.DrawIcon(ref x, ref y, x, y, 0.0f, 0.0f, trapOffset, trapOffset, textOffset, textOffset, iconSize, statistics.TrapsTotal);

                if (statistics.TimeLastFloor != default || statistics.ScoreLastFloor != 0)
                {
                    x = baseX + (floorWidth * 2);
                    y = baseY;
                    this.DrawFloorText(x, y, $"Last Floor:", statistics.TimeLastFloor, null, statistics.ScoreLastFloor, null, isSummary);
                    this.DrawIcon(ref x, ref y, x, y, 0.0f, 20.0f, miscellaneousOffset, miscellaneousOffset, textOffset, textOffset, iconSize, statistics.MiscellaneousLastFloor);
                    this.DrawIcon(ref x, ref y, x, y, 0.0f, 0.0f, pomanderOffset, pomanderOffset, textOffset, textOffset, iconSize, statistics.PomandersLastFloor);
                }
            }
            else
                ui.DrawTextAxisLatinPro(width / 2.0f, (height / 2.0f) + 15.0f, $"No data on {statistics.FloorSetStatistics.GetDescription().ToLower()}", Color.White, Alignment.Center);

            this.Size = new(width * ui.Scale, height * ui.Scale);
        }

        private void DrawFloorText(float x, float y, string floorText, TimeSpan totalTime, TimeSpan? previousTime, int totalScore, int? previousScore, bool forceShowHours = false)
        {
            var config = this.Configuration.Statistics;
            var ui = this.Data.UI;

            var lineHeight = 28.0f;
            var space = "   ";
            floorText += space;
            var totalTimeText = (totalTime.Hours > 0 || forceShowHours) ? $"{totalTime}" : $"{totalTime:mm\\:ss}";
            totalTimeText += space;

            var previousTimeText = "+";
            previousTimeText += previousTime?.Hours > 0 ? $"{previousTime}" : $"{previousTime:mm\\:ss}";

            ui.DrawTextAxisLatinPro(x, y, floorText, Color.White);

            var floorTextSize = ui.GetAxisLatinProTextSize(floorText);
            x += floorTextSize.X;
            ui.DrawTextAxisLatinPro(x, y, totalTimeText, config.FloorTimeColor);

            if (totalTime != previousTime && previousTime != null)
                ui.DrawTextAxisLatinPro(x, y + lineHeight, previousTimeText, config.FloorTimeColor);

            var totalTimeTextSize = ui.GetAxisLatinProTextSize(totalTimeText);
            x += totalTimeTextSize.X;
            ui.DrawTextAxisLatinPro(x, y, $"{totalScore:N0}", totalScore > 0 ? config.ScoreColor : totalScore < 0 ? Color.Red : Color.White);

            if (totalScore != previousScore && previousScore != null)
                ui.DrawTextAxisLatinPro(x, y + lineHeight, $"{(previousScore > 0 ? "+" : string.Empty)}{previousScore:N0}", previousScore > 0 ? config.ScoreColor : previousScore < 0 ? Color.Red : Color.White);
        }

        private void DrawIcon<T>(ref float x, ref float y, float left, float top, float offsetX, float offsetY, float iconOffsetX, float iconOffsetY, float textOffsetX, float textOffsetY, float iconSize, IEnumerable<DataStatistics.StatisticsItem<T>>? data, MapData? mapData = null) where T : Enum
        {
            var ui = this.Data.UI;
            x = left + offsetX;
            y = top + offsetY;
            foreach (var item in data ?? Enumerable.Empty<DataStatistics.StatisticsItem<T>>())
            {
                var value = item.Value;
                var total = item.Total;

                var iconX = x + iconOffsetX;
                var iconY = y + iconOffsetY;

                if (typeof(T) == typeof(Coffer))
                    ui.DrawCoffer(iconX, iconY, (Coffer)(Enum)value);
                else if (typeof(T) == typeof(Enchantment))
                    ui.DrawEnchantment(iconX, iconY, (Enchantment)(Enum)value);
                else if (typeof(T) == typeof(Trap))
                    ui.DrawTrap(iconX, iconY, (Trap)(Enum)value);
                else if (typeof(T) == typeof(Pomander))
                    ui.DrawPomander(iconX, iconY, (Pomander)(Enum)value);
                else
                {
                    var miscellaneousValue = (Miscellaneous)(Enum)item.Value;
                    if (mapData != null && miscellaneousValue == Miscellaneous.Map && mapData.FloorType != FloorType.None)
                    {
                        for (var j = 0; j < MapData.Length; j++)
                        {
                            for (var i = 0; i < MapData.Length; i++)
                            {
                                var size = (44.0f / 9.0f) * ((mapData.FloorType == FloorType.Normal) ? 2.0f : 1.0f);
                                var posX = x + (i * size);
                                var posY = y + (j * size);
                                var id = mapData.GetId(i, j);

                                if (id == null)
                                    continue;

                                if (mapData.FloorType == FloorType.Normal)
                                    ui.DrawMapNormal(posX, posY, id.Value);
                                else if (mapData.FloorType == FloorType.HallOfFallacies)
                                    ui.DrawMapHallOfFallacies(posX, posY, id.Value);
                            }
                        }
                    }
                    else
                        ui.DrawMiscellaneous(iconX, iconY, miscellaneousValue);
                }

                if (total > 1)
                    ui.DrawTextAxisLatinPro(x + textOffsetX + iconSize, y + textOffsetY + iconSize, total.ToString(), Color.White, Alignment.Right, true);

                x += iconSize;
            }
        }
    }
}