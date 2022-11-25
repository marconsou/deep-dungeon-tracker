using System;
using System.Collections.Generic;
using System.Numerics;

namespace DeepDungeonTracker
{
    public sealed class FloorSetTimeWindow : WindowEx, IDisposable
    {
        private Data Data { get; }

        public FloorSetTimeWindow(string id, Configuration configuration, Data data) : base(id, configuration) => this.Data = data;

        public void Dispose() { }

        public override void PreOpenCheck()
        {
            var config = this.Configuration.FloorSetTime;
            this.IsOpen = this.Data.UI.CommonWindowVisibility(config.Show, config.ShowInBetweenFloors, this.Data.Common.IsInDeepDungeonRegion, this.Data.IsInsideDeepDungeon);
            this.Flags = config.Lock ? WindowEx.StaticNoBackground : WindowEx.StaticNoBackgroundMoveInputs;
        }

        public override void Draw()
        {
            var dataCommon = this.Data.Common;
            var config = this.Configuration.FloorSetTime;
            var ui = this.Data.UI;
            ui.Scale = config.Scale;
            var left = 15.0f;
            var top = 50.0f;
            var lineHeight = 30.0f;
            var numberOfLines = config.ShowFloorTime ? 5 : 0;
            var width = 275.0f;
            var height = (top + (lineHeight * numberOfLines) - 3.0f) + 37.0f + (!config.ShowFloorTime ? -7.0f : 0.0f);

            void DrawTextLine(float x, float y, string label, TimeSpan? value, Vector4 labelColor, Vector4 valueColor)
            {
                ui.DrawTextAxisLatinPro(x, y, label, labelColor, Align.Left);

                var size = ui.GetAxisLatinProTextSize(label + " ");

                if (value != null)
                    ui.DrawTextAxisLatinPro(x + size.X, y, $"{value:mm\\:ss}", valueColor, Align.Left);
            }

            ui.DrawBackground(width, height, (!this.Configuration.General.SolidBackgroundWindow && this.IsFocused) || this.Configuration.General.SolidBackgroundWindow);
            ui.DrawTextMiedingerMediumW00(width / 2.0f, 20.0f, "Floor Set Time", Color.White, Align.Center);
            ui.DrawDivisorHorizontal(14.0f, 34.0f, width - 26.0f);

            var x = left;
            var y = top;

            var showValue = (this.Data.IsInsideDeepDungeon || dataCommon.ContentId != 0);
            if (config.ShowFloorTime)
            {
                var number = dataCommon.CurrentSaveSlot?.CurrentFloorSet()?.FirstFloor()?.Number ?? null;
                var floorsTime = new List<TimeSpan>(dataCommon.FloorSetTime.PreviousFloorsTime) { dataCommon.FloorSetTime.CurrentFloorTime };
                for (var i = 0; i < 10; i++)
                {
                    TimeSpan? floorTime = i < floorsTime.Count ? floorsTime[i] : null;
                    var isCurrentFloor = (i == floorsTime.Count - 1);

                    DrawTextLine(x, y, $"{(number.HasValue ? $"{number:D3}" : " ??? ")}:", showValue ? floorTime : null, Color.White, !isCurrentFloor ? config.PreviousFloorTimeColor : config.CurrentFloorTimeColor);

                    if (i % 2 != 0)
                    {
                        y += lineHeight;
                        x = left;
                    }
                    else
                        x = (width / 2.0f);

                    if (number.HasValue)
                        number++;
                }

                x = left;
                y -= 8.0f;
                ui.DrawDivisorHorizontal(14.0f, y, width - 26.0f);
                y += 15.0f;
            }

            DrawTextLine(x, y, "Average:", showValue ? dataCommon.FloorSetTime.Average : null, Color.White, config.AverageTimeColor);

            x = width / 2.0f;
            var currentFloorTime = dataCommon.FloorSetTime.CurrentFloorTime;
            var currentFloor = dataCommon.CurrentSaveSlot?.CurrentFloor() ?? default;

            if (!currentFloor?.IsFirstFloor() ?? false)
                currentFloorTime = currentFloorTime.Add(new(0, 0, 0, 0, -600));

            var respawnTime = dataCommon.GetRespawnTime();
            var value = respawnTime - TimeSpan.FromTicks((currentFloorTime.Ticks) % (respawnTime.Ticks + 1));
            DrawTextLine(x, y, "Respawn:", (!currentFloor?.IsLastFloor() ?? false) && showValue ? value : null, Color.White, config.RespawnTimeColor);

            this.Size = new(width * ui.Scale, height * ui.Scale);
        }
    }
}