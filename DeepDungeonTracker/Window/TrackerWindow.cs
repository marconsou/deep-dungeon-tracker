using System;
using System.Linq;

namespace DeepDungeonTracker
{
    public sealed class TrackerWindow : WindowEx, IDisposable
    {
        private Data Data { get; }

        private Action<float, float, float, SaveSlot, FloorSet, Floor>?[]? FieldCalls { get; set; }

        public TrackerWindow(string id, Configuration configuration, Data data) : base(id, configuration) => this.Data = data;

        public void Dispose() { }

        public override void PreOpenCheck()
        {
            var config = this.Configuration.Tracker;
            this.IsOpen = this.Data.UI.CommonWindowVisibility(config.Show, config.ShowInBetweenFloors, this.Data.Common.IsInDeepDungeonRegion, this.Data.IsInsideDeepDungeon);
            this.Flags = config.Lock ? WindowEx.StaticNoBackground : WindowEx.StaticNoBackgroundMoveInputs;
        }

        public override void Draw()
        {
            void Kills(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Kills:", floor.Kills, floorSet.Kills(), saveSlot.Kills());

            void Mimics(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Mimics:", floor.Mimics, floorSet.Mimics(), saveSlot.Mimics());

            void Mandragoras(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Mandragoras:", floor.Mandragoras, floorSet.Mandragoras(), saveSlot.Mandragoras());

            void Mimicgoras(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Mimicgoras:", floor.Mimics + floor.Mandragoras, floorSet.Mimics() + floorSet.Mandragoras(), saveSlot.Mimics() + saveSlot.Mandragoras());

            void NPCs(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "NPCs:", floor.NPCs, floorSet.NPCs(), saveSlot.NPCs());

            void Coffers(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Coffers:", floor.Coffers.Count, floorSet.Coffers(), saveSlot.Coffers());

            void Enchantments(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Enchantments:", floor.Enchantments.Count, floorSet.Enchantments(), saveSlot.Enchantments());

            void Traps(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Traps:", floor.Traps.Count, floorSet.Traps(), saveSlot.Traps());

            void Deaths(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Deaths:", floor.Deaths, floorSet.Deaths(), saveSlot.Deaths());

            void RegenPotions(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Regen Potions:", floor.RegenPotions, floorSet.RegenPotions(), saveSlot.RegenPotions());

            void Potsherds(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Potsherds:", floor.Potsherds(), floorSet.Potsherds(), saveSlot.Potsherds());

            void Lurings(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Lurings:", floor.Lurings(), floorSet.Lurings(), saveSlot.Lurings());

            void Maps(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Maps:", Convert.ToInt32(floor.Map), floorSet.Maps(), saveSlot.Maps(), true);

            void TimeBonuses(float y, float columnX, float offsetX, SaveSlot saveSlot, FloorSet floorSet, Floor floor)
                => DrawTextLine(y, columnX, offsetX, "Time Bonuses:", int.MinValue, Convert.ToInt32(floorSet.TimeBonus), saveSlot.TimeBonuses(), false, true);

            var config = this.Configuration.Tracker;
            var ui = this.Data.UI;
            ui.Scale = config.Scale;
            var left = 15.0f;
            var top = 50.0f;
            var lineHeight = 30.0f;
            var NPCindex = 4;
            var ShowNPC = (config.Fields?[NPCindex].Show ?? false) &&
                ((this.Data.Common.CurrentSaveSlot?.DeepDungeon == DeepDungeon.PalaceOfTheDead) ||
                (this.Data.Common.CurrentSaveSlot?.DeepDungeon == DeepDungeon.None && this.Data.Common.DeepDungeon == DeepDungeon.PalaceOfTheDead));
            var numberOfLines = (config.Fields?.Where((x, Index) => Index != NPCindex).Count(x => x.Show) ?? 0) + (ShowNPC ? 1 : 0);
            var width = 380.0f;
            var height = (top + (lineHeight * numberOfLines) - 3.0f);
            var columnX = 170;
            var spacing = 5.0f;
            var offsetX = 76.0f + spacing;
            width = columnX - 27.0f + (Convert.ToInt32(config.IsFloorNumberVisible) + Convert.ToInt32(config.IsSetNumberVisible) + Convert.ToInt32(config.IsTotalNumberVisible)) * offsetX;

            this.FieldCalls = new[] { Kills, Mimics, Mandragoras, Mimicgoras, ShowNPC ? NPCs : null, Coffers, Enchantments, Traps, Deaths, RegenPotions, Potsherds, Lurings, Maps, TimeBonuses };

            ui.DrawBackground(width, height, (!this.Configuration.General.SolidBackgroundWindow && this.IsFocused) || this.Configuration.General.SolidBackgroundWindow);

            var x = left;
            var y = top;
            x = columnX;

            if (config.IsFloorNumberVisible)
            {
                ui.DrawTextMiedingerMediumW00(x, 20.0f, "Floor", Color.White, Alignment.Center);
                x += offsetX;
            }
            if (config.IsSetNumberVisible)
            {
                ui.DrawTextMiedingerMediumW00(x, 20.0f, "Set", Color.White, Alignment.Center);
                x += offsetX;
            }
            if (config.IsTotalNumberVisible)
                ui.DrawTextMiedingerMediumW00(x, 20.0f, "Total", Color.White, Alignment.Center);

            ui.DrawDivisorHorizontal(14.0f, 34.0f, width - 26.0f);

            var saveSlot = this.Data.Common.CurrentSaveSlot ?? new();
            var floorSet = saveSlot.CurrentFloorSet() ?? new();
            var floor = saveSlot.CurrentFloor() ?? new(0);

            for (var i = 0; i < config.Fields?.Length; i++)
            {
                var field = config.Fields[i];
                var fieldCall = this.FieldCalls[field.Index];
                if (field.Show)
                {
                    fieldCall?.Invoke(y, columnX, offsetX, saveSlot, floorSet, floor);
                    if (fieldCall != null)
                        y += lineHeight;
                }
            }

            var floorEffectPomandersScale = 0.5f;
            ui.Scale *= floorEffectPomandersScale;
            if (config.ShowFloorEffectPomanders && !floor.IsLastFloor())
            {
                var multiplier = 1.0f / floorEffectPomandersScale;
                x = left * multiplier;
                y = 11.0f * multiplier;
                var spaceX = 28.0f * multiplier;

                if (this.Data.Common.FloorEffect.ShowPomanderOfSafety)
                {
                    ui.DrawCoffer(x, y, Coffer.PomanderOfSafety);
                    x += spaceX;
                }

                if (this.Data.Common.FloorEffect.ShowPomanderOfAffluence)
                {
                    ui.DrawCoffer(x, y, Coffer.PomanderOfAffluence);
                    x += spaceX;
                }

                if (this.Data.Common.FloorEffect.ShowPomanderOfFlight)
                {
                    ui.DrawCoffer(x, y, Coffer.PomanderOfFlight);
                    x += spaceX;
                }

                if (this.Data.Common.FloorEffect.ShowPomanderOfAlteration)
                    ui.DrawCoffer(x, y, Coffer.PomanderOfAlteration);
            }
            ui.Scale /= floorEffectPomandersScale;

            this.Size = new(width * ui.Scale, height * ui.Scale);
        }

        void DrawTextLine(float y, float columnX, float offsetX, string label, int floorValue, int setValue, int totalValue, bool floorValueAsIcon = false, bool setValueAsIcon = false)
        {
            var config = this.Configuration.Tracker;
            var ui = this.Data.UI;
            var numberOffsetY = 7.0f;
            var iconOffsetY = 7.0f;
            var x = columnX;
            ui.DrawTextAxisLatinPro(15.0f, y, label, Color.White, Alignment.Left);
            if (config.IsFloorNumberVisible)
            {
                var valueX = x;
                var valueY = y;
                if (floorValue >= 0)
                {
                    if (!floorValueAsIcon)
                        ui.DrawNumber(valueX, valueY + numberOffsetY, floorValue, false, config.FloorNumberColor, Alignment.Center);
                    else
                        ui.DrawCheckMark(valueX, valueY + iconOffsetY, floorValue == 1);
                }
                x += offsetX;
            }
            if (config.IsSetNumberVisible)
            {
                var valueX = x;
                var valueY = y;
                if (setValue >= 0)
                {
                    if (!setValueAsIcon)
                        ui.DrawNumber(valueX, valueY + numberOffsetY, setValue, false, config.SetNumberColor, Alignment.Center);
                    else
                        ui.DrawCheckMark(valueX, valueY + iconOffsetY, setValue == 1);
                }
                x += offsetX;
            }
            if (config.IsTotalNumberVisible)
            {
                var valueX = x;
                var valueY = y;
                if (totalValue >= 0)
                    ui.DrawNumber(valueX, valueY + numberOffsetY, totalValue, false, config.TotalNumberColor, Alignment.Center);
            }
        }
    }
}