using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;

namespace DeepDungeonTracker
{
    public class DataStatistics
    {
        public record StatisticsItem<T>(T Value, int Total);

        public bool Open { get; set; }

        public FloorSetStatistics FloorSetStatistics { get; set; } = FloorSetStatistics.Summary;

        private SaveSlot? SaveSlot { get; set; }

        public FloorSet? FloorSet { get; private set; }

        public IEnumerable<FloorSet>? FloorSets { get; private set; }

        public IImmutableList<IEnumerable<StatisticsItem<Miscellaneous>>>? MiscellaneousByFloor { get; private set; }

        public IImmutableList<IEnumerable<StatisticsItem<Coffer>>>? CoffersByFloor { get; private set; }

        public IImmutableList<IEnumerable<StatisticsItem<Enchantment>>>? EnchantmentsByFloor { get; private set; }

        public IImmutableList<IEnumerable<StatisticsItem<Trap>>>? TrapsByFloor { get; private set; }

        public IImmutableList<IEnumerable<StatisticsItem<Pomander>>>? PomandersByFloor { get; private set; }

        public IEnumerable<StatisticsItem<Miscellaneous>>? MiscellaneousLastFloor { get; private set; }

        public IEnumerable<StatisticsItem<Miscellaneous>>? MiscellaneousTotal { get; private set; }

        public IEnumerable<StatisticsItem<Coffer>>? CoffersTotal { get; private set; }

        public IEnumerable<StatisticsItem<Enchantment>>? EnchantmentsTotal { get; private set; }

        public IEnumerable<StatisticsItem<Trap>>? TrapsTotal { get; private set; }

        public IEnumerable<StatisticsItem<Pomander>>? PomandersLastFloor { get; private set; }

        public IEnumerable<StatisticsItem<Pomander>>? PomandersTotal { get; private set; }

        public TimeSpan TimeLastFloor { get; private set; }

        public TimeSpan TimeTotal { get; private set; }

        public int ScoreLastFloor { get; private set; }

        public int ScoreTotal { get; private set; }

        public uint ClassJobId => this.SaveSlot?.ClassJobId ?? 0;

        public void FloorSetStatisticsPrevious()
        {
            var value = ((int)this.FloorSetStatistics) - 1;
            var enumValues = Enum.GetValues(typeof(FloorSetStatistics)).Cast<FloorSetStatistics>().ToImmutableArray();
            this.FloorSetStatistics = value < (int)enumValues.Min() ? enumValues.Max() : (FloorSetStatistics)value;
            this.DataUpdate();
        }

        public void FloorSetStatisticsNext()
        {
            var value = ((int)this.FloorSetStatistics) + 1;
            var enumValues = Enum.GetValues(typeof(FloorSetStatistics)).Cast<FloorSetStatistics>().ToImmutableArray();
            this.FloorSetStatistics = value > (int)enumValues.Max() ? enumValues.Min() : (FloorSetStatistics)value;
            this.DataUpdate();
        }

        public void Load(SaveSlot? saveSlot)
        {
            this.SaveSlot = saveSlot;
            this.Open = true;
            this.DataUpdate();
        }

        public void DataUpdate()
        {
            this.FloorSet = null;
            this.FloorSets = null;

            this.MiscellaneousByFloor = ImmutableArray<IEnumerable<StatisticsItem<Miscellaneous>>>.Empty;
            this.CoffersByFloor = ImmutableArray<IEnumerable<StatisticsItem<Coffer>>>.Empty;
            this.EnchantmentsByFloor = ImmutableArray<IEnumerable<StatisticsItem<Enchantment>>>.Empty;
            this.TrapsByFloor = ImmutableArray<IEnumerable<StatisticsItem<Trap>>>.Empty;
            this.PomandersByFloor = ImmutableArray<IEnumerable<StatisticsItem<Pomander>>>.Empty;

            if (this.FloorSetStatistics != FloorSetStatistics.Summary)
            {
                this.FloorSet = this.SaveSlot?.FloorSets.FirstOrDefault(x => x.FirstFloor()?.Number == ((int)this.FloorSetStatistics * 10) - 9);

                this.MiscellaneousByFloor = DataStatistics.GetMiscellaneousByFloorsList(this.FloorSet?.Floors ?? new());
                this.CoffersByFloor = this.CoffersByFloor.AddRange(this.FloorSet?.Floors.Select(x => x.Coffers.GroupBy(x => x).Select(x => new StatisticsItem<Coffer>(x.Key, x.Count())).Take(9)) ?? ImmutableArray<IEnumerable<StatisticsItem<Coffer>>>.Empty);
                this.EnchantmentsByFloor = this.EnchantmentsByFloor.AddRange(this.FloorSet?.Floors.Select(x => x.AdjustedEnchantments().GroupBy(x => x).Select(x => new StatisticsItem<Enchantment>(x.Key, x.Count())).Take(3)) ?? ImmutableArray<IEnumerable<StatisticsItem<Enchantment>>>.Empty);
                this.TrapsByFloor = this.TrapsByFloor.AddRange(this.FloorSet?.Floors.Select(x => x.Traps.GroupBy(x => x).Select(x => new StatisticsItem<Trap>(x.Key, x.Count())).Take(6)) ?? ImmutableArray<IEnumerable<StatisticsItem<Trap>>>.Empty);
                this.PomandersByFloor = this.PomandersByFloor.AddRange(this.FloorSet?.Floors.Select(x => x.Pomanders.GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).Take(9)) ?? ImmutableArray<IEnumerable<StatisticsItem<Pomander>>>.Empty);

                this.MiscellaneousLastFloor = (this.MiscellaneousByFloor?.Count == 10) ? this.MiscellaneousByFloor[^1] : default;
                this.MiscellaneousTotal = DataStatistics.GetMiscellaneousByFloorSet(this.FloorSet);

                this.CoffersTotal = this.FloorSet?.Floors.SelectMany(x => x.Coffers).GroupBy(x => x).Select(x => new StatisticsItem<Coffer>(x.Key, x.Count()));
                this.EnchantmentsTotal = this.FloorSet?.Floors.SelectMany(x => x.Enchantments).GroupBy(x => x).Select(x => new StatisticsItem<Enchantment>(x.Key, x.Count()));
                this.TrapsTotal = this.FloorSet?.Floors.SelectMany(x => x.Traps).GroupBy(x => x).Select(x => new StatisticsItem<Trap>(x.Key, x.Count()));

                this.PomandersLastFloor = this.FloorSet?.LastFloor()?.Pomanders.GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).Take(9 - (this.MiscellaneousLastFloor?.Count() ?? 0));
                this.PomandersTotal = this.FloorSet?.Floors.SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count()));

                this.TimeLastFloor = new(this.FloorSet?.LastFloor()?.Time.Ticks ?? default);
                this.TimeTotal = new(this.FloorSet?.Floors.Sum(x => x.Time.Ticks) ?? default);

                this.ScoreLastFloor = this.FloorSet?.LastFloor()?.Score ?? 0;
                this.ScoreTotal = this.FloorSet?.Score() ?? 0;
            }
            else
            {
                this.FloorSets = this.SaveSlot?.FloorSets;

                var floors = this.FloorSets?.SelectMany(x => x.Floors);
                var lastFloors = floors?.Where(x => x.IsLastFloor());

                this.MiscellaneousLastFloor = DataStatistics.GetMiscellaneousByFloors(lastFloors);
                this.MiscellaneousTotal = DataStatistics.GetMiscellaneousBySaveSlot(this.SaveSlot);

                this.CoffersTotal = floors?.SelectMany(x => x.Coffers).GroupBy(x => x).Select(x => new StatisticsItem<Coffer>(x.Key, x.Count()));
                this.EnchantmentsTotal = floors?.SelectMany(x => x.Enchantments).GroupBy(x => x).Select(x => new StatisticsItem<Enchantment>(x.Key, x.Count()));
                this.TrapsTotal = floors?.SelectMany(x => x.Traps).GroupBy(x => x).Select(x => new StatisticsItem<Trap>(x.Key, x.Count()));

                this.PomandersLastFloor = lastFloors?.SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).Take(9 - (this.MiscellaneousLastFloor?.Count() ?? 0));
                this.PomandersTotal = floors?.SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count()));

                this.TimeLastFloor = new(lastFloors?.Sum(x => x.Time.Ticks) ?? default);
                this.TimeTotal = this.SaveSlot?.Time() ?? default;

                this.ScoreLastFloor = lastFloors?.Sum(x => x.Score) ?? 0;
                this.ScoreTotal = this.SaveSlot?.Score() ?? 0;
            }
        }

        private static IEnumerable<StatisticsItem<Miscellaneous>>? GetMiscellaneousByFloors(IEnumerable<Floor>? floors)
        {
            return ImmutableArray.CreateRange(new StatisticsItem<Miscellaneous>[]
            {
                new (Miscellaneous.Enemy, floors?.Sum(x=> x.Kills) ?? 0),
                new (Miscellaneous.CairnOfPassageEnemy, floors?.Sum(x=> x.CairnOfPassageKills) ?? 0),
                new (Miscellaneous.Mimic, floors?.Sum(x=> x.Mimics) ?? 0),
                new (Miscellaneous.Mandragora, floors?.Sum(x=> x.Mandragoras) ?? 0),
                new (Miscellaneous.NPC, floors?.Sum(x=> x.NPCs) ?? 0),
                new (Miscellaneous.Death, floors?.Sum(x=> x.Deaths) ?? 0),
                new (Miscellaneous.RegenPotion, floors?.Sum(x=> x.RegenPotions) ?? 0),
                new (Miscellaneous.Map, floors?.Sum(x => Convert.ToInt32(x.Map)) ?? 0)
            }).RemoveAll(x => x.Total == 0);
        }

        private static IImmutableList<IEnumerable<StatisticsItem<Miscellaneous>>>? GetMiscellaneousByFloorsList(IEnumerable<Floor>? floors)
        {
            static IEnumerable<StatisticsItem<Miscellaneous>> GetStatisticsCommonFloor(Floor? floor)
            {
                return ImmutableArray.CreateRange(new StatisticsItem<Miscellaneous>[]
                {
                    new (Miscellaneous.Enemy, floor?.Kills ?? 0),
                    new (Miscellaneous.CairnOfPassageEnemy, floor?.CairnOfPassageKills ?? 0),
                    new (Miscellaneous.Mimic, floor?.Mimics ?? 0),
                    new (Miscellaneous.Mandragora, floor?.Mandragoras ?? 0),
                    new (Miscellaneous.NPC, floor?.NPCs ?? 0),
                    new (Miscellaneous.Death, floor?.Deaths ?? 0),
                    new (Miscellaneous.RegenPotion, floor?.RegenPotions ?? 0),
                    new (Miscellaneous.Map, Convert.ToInt32(floor?.Map, CultureInfo.InvariantCulture))
                }).RemoveAll(x => x.Total == 0);
            }
            return floors?.SelectMany(x => ImmutableArray.Create(GetStatisticsCommonFloor(x))).ToImmutableList();
        }

        private static IEnumerable<StatisticsItem<Miscellaneous>>? GetMiscellaneousByFloorSet(FloorSet? floorSet)
        {
            return ImmutableArray.CreateRange(new StatisticsItem<Miscellaneous>[]
            {
                new (Miscellaneous.Enemy, floorSet?.Kills() ?? 0),
                new (Miscellaneous.CairnOfPassageEnemy, floorSet?.CairnOfPassageKills() ?? 0),
                new (Miscellaneous.Mimic, floorSet?.Mimics() ?? 0),
                new (Miscellaneous.Mandragora, floorSet?.Mandragoras() ?? 0),
                new (Miscellaneous.NPC, floorSet?.NPCs() ?? 0),
                new (Miscellaneous.Death, floorSet?.Deaths() ?? 0),
                new (Miscellaneous.RegenPotion, floorSet?.RegenPotions() ?? 0),
                new (Miscellaneous.Map, floorSet?.Maps() ?? 0),
                new (Miscellaneous.TimeBonus, Convert.ToInt32(floorSet?.TimeBonus, CultureInfo.InvariantCulture))
            }).RemoveAll(x => x.Total == 0);
        }

        private static IEnumerable<StatisticsItem<Miscellaneous>>? GetMiscellaneousBySaveSlot(SaveSlot? saveSlot)
        {
            return ImmutableArray.CreateRange(new StatisticsItem<Miscellaneous>[]
            {
                new (Miscellaneous.Enemy, saveSlot?.Kills() ?? 0),
                new (Miscellaneous.CairnOfPassageEnemy, saveSlot?.CairnOfPassageKills() ?? 0),
                new (Miscellaneous.Mimic, saveSlot?.Mimics() ?? 0),
                new (Miscellaneous.Mandragora, saveSlot?.Mandragoras() ?? 0),
                new (Miscellaneous.NPC, saveSlot?.NPCs() ?? 0),
                new (Miscellaneous.Death, saveSlot?.Deaths() ?? 0),
                new (Miscellaneous.RegenPotion, saveSlot?.RegenPotions() ?? 0),
                new (Miscellaneous.Map, saveSlot?.Maps() ?? 0),
                new (Miscellaneous.TimeBonus, saveSlot?.TimeBonuses() ?? 0)
            }).RemoveAll(x => x.Total == 0);
        }
    }
}