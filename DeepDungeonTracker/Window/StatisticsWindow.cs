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

        public StatisticsWindow(string id, Configuration configuration, Data data) : base(id, configuration, WindowEx.StaticNoBackgroundMoveInputs)
        {
            this.Data = data;
            this.IsOpen = true;
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
            var statistics = this.Data.Statistics;
            var ui = this.Data.UI;
            ui.Scale = this.Configuration.Statistics.Scale;
            var left = 15.0f;
            var top = 50.0f;
            var width = 1359.0f;
            var height = 797.0f;
            var floorWidth = 450.0f;
            var floorHeight = 189.0f;
            var floorSet = statistics.FloorSet;
            var floorSets = statistics.FloorSets;
            var noData = (floorSet == null && floorSets == null);

            if (noData)
                height = 230.0f;
            else if (statistics.FloorSetStatistics == FloorSetStatistics.AllFloors)
                height = 230.0f;

            ui.DrawBackground(width, height, (!this.Configuration.General.SolidBackgroundWindow && this.IsFocused) || this.Configuration.General.SolidBackgroundWindow);
            ui.DrawTextMiedingerMediumW00(width / 2.0f, 20.0f, "Statistics", Color.White, Align.Center);
            ui.DrawDivisorHorizontal(14.0f, 34.0f, width - 26.0f);

            this.ArrowButtonPrevious.Position = new((width / 2.0f) - 108.0f, 7.0f);
            this.ArrowButtonNext.Position = new((width / 2.0f) + 77.0f, 7.0f);
            this.CloseButton.Position = new(width - 35.0f, 7.0f);
            this.ArrowButtonPrevious.Draw(ui);
            this.ArrowButtonNext.Draw(ui);
            this.CloseButton.Draw(ui);

            if (!noData)
            {
                var x = left;
                var y = top;
                var statisticsCommonOffset = 0.0f;
                var cofferOffset = 4.0f;
                var enchantmentOffset = 0.0f;
                var trapOffset = 8.0f;
                var textOffset = -4.0f;
                var iconSize = 48.0f;
                var isAllFloors = statistics.FloorSetStatistics == FloorSetStatistics.AllFloors;
                float baseX;
                float baseY;
                if (!isAllFloors)
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
                            ui.DrawTextAxisLatinPro(baseX, baseY, $"Floor {floor.Number} ({floor.Time:mm\\:ss}) {floor.Score}pts", Color.White);

                            this.DrawIcon(ref x, ref y, baseX, y, 0.0f, 20.0f, statisticsCommonOffset, statisticsCommonOffset, textOffset, textOffset, iconSize, i < statistics.MiscellaneousByFloor?.Count ? statistics.MiscellaneousByFloor[i] : default, floor.MapData);
                            this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, cofferOffset, cofferOffset, textOffset, textOffset, iconSize, i < statistics.CoffersByFloor?.Count ? statistics.CoffersByFloor[i] : default);
                            this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, enchantmentOffset, enchantmentOffset, textOffset, textOffset, iconSize, i < statistics.EnchantmentsByFloor?.Count ? statistics.EnchantmentsByFloor[i] : default);
                            this.DrawIcon(ref x, ref y, x, y, 0.0f, 0.0f, trapOffset, trapOffset, textOffset, textOffset, iconSize, i < statistics.TrapsByFloor?.Count ? statistics.TrapsByFloor[i] : default);
                        }

                        ui.DrawDivisorVertical(baseX + floorWidth - 13.0f, baseY - 13.0f, floorHeight + 1.0f);

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

                x = left;
                var totalTime = !isAllFloors ? $"{statistics.TotalTime:mm\\:ss}" : $"{statistics.TotalTime}";
                ui.DrawTextAxisLatinPro(x, y, $"Total ({totalTime}) {statistics.TotalScore}pts", Color.White);

                baseX = x;
                baseY = y;
                this.DrawIcon(ref x, ref y, baseX, y, 0.0f, 20.0f, statisticsCommonOffset, statisticsCommonOffset, textOffset, textOffset, iconSize, statistics.MiscellaneousTotal);
                this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, cofferOffset, cofferOffset, textOffset, textOffset, iconSize, statistics.CoffersTotal);
                this.DrawIcon(ref x, ref y, baseX, y, 0.0f, iconSize, enchantmentOffset, enchantmentOffset, textOffset, textOffset, iconSize, statistics.EnchantmentsTotal);
                this.DrawIcon(ref x, ref y, x, y, 0.0f, 0.0f, trapOffset, trapOffset, textOffset, textOffset, iconSize, statistics.TrapsTotal);

                if (statistics.LastFloorTotal?.Count() > 0)
                {
                    x = baseX + (floorWidth * 2);
                    y = baseY;
                    var lastFloorTime = !isAllFloors ? $"{statistics.LastFloorTime:mm\\:ss}" : $"{statistics.LastFloorTime}";
                    ui.DrawTextAxisLatinPro(x, y, $"Last Floor ({lastFloorTime}) {statistics.LastFloorScore}pts", Color.White);
                    this.DrawIcon(ref x, ref y, x, y, 0.0f, 20.0f, statisticsCommonOffset, statisticsCommonOffset, textOffset, textOffset, iconSize, statistics.LastFloorTotal);
                }
            }
            else
                ui.DrawTextAxisLatinPro(width / 2.0f, (height / 2.0f) + 15.0f, $"No data on {statistics.FloorSetStatistics.GetDescription().ToLower()}", Color.White, Align.Center);

            this.Size = new(width * ui.Scale, height * ui.Scale);
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
                else
                {
                    var miscellaneousValue = (Miscellaneous)(Enum)item.Value;
                    if (mapData != null && miscellaneousValue == Miscellaneous.Map && mapData?.FloorType != FloorType.None)
                    {
                        for (var j = 0; j < MapData.GetLength(); j++)
                        {
                            for (var i = 0; i < MapData.GetLength(); i++)
                            {
                                var size = (44.0f / 9.0f) * ((mapData?.FloorType == FloorType.Normal) ? 2.0f : 1.0f);
                                var posX = x + (i * size);
                                var posY = y + (j * size);
                                var id = mapData?.GetId(i, j);

                                if (id == null)
                                    continue;

                                if (mapData?.FloorType == FloorType.Normal)
                                    ui.DrawMapNormal(posX, posY, id.Value);
                                else if (mapData?.FloorType == FloorType.HallOfFallacies)
                                    ui.DrawMapHallOfFallacies(posX, posY, id.Value);
                            }
                        }
                    }
                    else
                        ui.DrawMiscellaneous(iconX, iconY, miscellaneousValue);
                }

                if (total > 1)
                    ui.DrawTextAxisLatinPro(x + textOffsetX + iconSize, y + textOffsetY + iconSize, total.ToString(), Color.White, Align.Right, true);

                x += iconSize;
            }
        }
    }
}