using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Action = System.Action;

namespace DeepDungeonTracker;

public class DataStatistics
{
    public record StatisticsItem<T>(T Value, int Total);

    public FloorSetStatistics FloorSetStatistics { get; set; } = FloorSetStatistics.Summary;

    public string FloorSetTextSummary { get; private set; } = string.Empty;

    public SaveSlot? SaveSlot { get; private set; }

    public SaveSlot? SaveSlotSummary { get; private set; }

    public Score? ScoreSummary { get; private set; }

    public FloorSet? FloorSet { get; private set; }

    public IEnumerable<FloorSet>? FloorSets { get; private set; }

    public IImmutableList<IEnumerable<StatisticsItem<Miscellaneous>>>? MiscellaneousByFloor { get; private set; }

    public IImmutableList<IEnumerable<StatisticsItem<Coffer>>>? CoffersByFloor { get; private set; }

    public IImmutableList<IEnumerable<StatisticsItem<Enchantment>>>? EnchantmentsByFloor { get; private set; }

    public IImmutableList<IEnumerable<StatisticsItem<Trap>>>? TrapsByFloor { get; private set; }

    public IImmutableList<IEnumerable<StatisticsItem<Pomander>>>? PomandersByFloor { get; private set; }

    public IEnumerable<StatisticsItem<Miscellaneous>>? MiscellaneousLastFloor { get; private set; }

    public IEnumerable<StatisticsItem<Miscellaneous>>? MiscellaneousTotal { get; private set; }

    public IEnumerable<StatisticsItem<BossStatusTimer>>? BossStatusTimerByFloorSet { get; private set; }

    public IEnumerable<StatisticsItem<Coffer>>? CoffersTotal { get; private set; }

    public IEnumerable<StatisticsItem<Enchantment>>? EnchantmentsTotal { get; private set; }

    public IEnumerable<StatisticsItem<Trap>>? TrapsTotal { get; private set; }

    public IEnumerable<StatisticsItem<Pomander>>? PomandersLastFloor { get; private set; }

    public IEnumerable<StatisticsItem<Pomander>>? PomandersTotal { get; private set; }

    public IEnumerable<StatisticsItem<Pomander>>? PomandersBossStatusTimer { get; private set; }

    public IEnumerable<StatisticsItem<Pomander>>? Inventory { get; private set; }

    public TimeSpan TimeLastFloor { get; private set; }

    public TimeSpan TimeTotal { get; private set; }

    public int ScoreLastFloor { get; private set; }

    public int ScoreTotal { get; private set; }

    public DeepDungeon DeepDungeon => this.SaveSlot?.DeepDungeon ?? DeepDungeon.None;

    public uint ClassJobId => this.SaveSlot?.ClassJobId ?? 0;

    public bool IsEurekaOrthosFloor99 => this.DeepDungeon == DeepDungeon.EurekaOrthos && this.FloorSetStatistics == FloorSetStatistics.From091To100;

    private static IEnumerable<Pomander> BossRelevantPomanders { get; }

    static DataStatistics()
    {
        DataStatistics.BossRelevantPomanders = new List<Pomander>()
        {
            Pomander.Resolution,
            Pomander.InfernoMagicite, Pomander.CragMagicite, Pomander.VortexMagicite, Pomander.ElderMagicite,
            Pomander.UneiDemiclone, Pomander.DogaDemiclone, Pomander.OnionKnightDemiclone
        };
    }

    public void FloorSetStatisticsSummary()
    {
        this.FloorSetStatistics = FloorSetStatistics.Summary;
        this.DataUpdate();
    }

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

    public void FloorSetStatisticsCurrent()
    {
        var value = FloorSetStatistics.From001To010;
        foreach (FloorSetStatistics item in Enum.GetValues(typeof(FloorSetStatistics)))
        {
            if (item == FloorSetStatistics.Summary)
                continue;

            var floorNumbers = item.GetDescription().Split('-');
            if (floorNumbers.Length == 2)
            {
                var firstFloorNumber = this.SaveSlot?.CurrentFloorSet()?.FirstFloor()?.Number ?? 0;
                if (firstFloorNumber == Convert.ToInt32(floorNumbers[0], CultureInfo.InvariantCulture))
                {
                    value = (FloorSetStatistics)(Convert.ToInt32(floorNumbers[1], CultureInfo.InvariantCulture) / 10);
                    break;
                }
            }
        }
        this.FloorSetStatistics = value;
        this.DataUpdate();
    }

    private void ResetSummarySelection()
    {
        this.FloorSetTextSummary = string.Empty;
        this.SaveSlotSummary = null;
        this.ScoreSummary = null;
    }

    public void FloorSetStatisticsSummarySelection(int maxFloor, string floorSetTextSummary, ScoreCalculationType scoreCalculationType)
    {
        if (this.FloorSetTextSummary != floorSetTextSummary)
        {
            this.FloorSetTextSummary = floorSetTextSummary;
            this.SaveSlotSummary = new();
            SaveSlot.Copy(this.SaveSlot, this.SaveSlotSummary, maxFloor);
            this.ScoreSummary = ScoreCreator.Create(this.SaveSlotSummary, true);
            this.ScoreSummary?.TotalScoreCalculation(ServiceUtility.IsSolo, scoreCalculationType);
        }
        else
            this.ResetSummarySelection();

        this.DataUpdate();
    }

    public void Load(SaveSlot? saveSlot, Action openStatisticsWindow)
    {
        this.SaveSlot = saveSlot;
        this.ResetSummarySelection();
        this.DataUpdate();
        openStatisticsWindow?.Invoke();
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
        this.BossStatusTimerByFloorSet = new List<StatisticsItem<BossStatusTimer>>();
        this.PomandersBossStatusTimer = new List<StatisticsItem<Pomander>>();
        this.Inventory = new List<StatisticsItem<Pomander>>();

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
            this.BossStatusTimerByFloorSet = DataStatistics.GetBossStatusTimerByFloorSet(this.FloorSet)?.Take(9);

            this.CoffersTotal = this.FloorSet?.Floors.SelectMany(x => x.Coffers).GroupBy(x => x).Select(x => new StatisticsItem<Coffer>(x.Key, x.Count())).OrderByDescending(x => x.Value != Coffer.Potsherd && x.Value != Coffer.Medicine && x.Value != Coffer.Aetherpool).ThenBy(x => x.Value).Take(22);
            this.EnchantmentsTotal = this.FloorSet?.Floors.SelectMany(x => x.Enchantments).GroupBy(x => x).Select(x => new StatisticsItem<Enchantment>(x.Key, x.Count()));
            this.TrapsTotal = this.FloorSet?.Floors.SelectMany(x => x.Traps).GroupBy(x => x).Select(x => new StatisticsItem<Trap>(x.Key, x.Count()));

            this.PomandersLastFloor = this.FloorSet?.LastFloor()?.Pomanders.GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).Take(9);
            this.PomandersTotal = this.FloorSet?.Floors.SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).OrderBy(x => x.Value).Take(22);

            var floorSetsExceptLast = this.SaveSlot?.FloorSets.Where(x => x.FirstFloor()?.Number < ((int)this.FloorSetStatistics * 10) - 9);
            var floorsExceptLast = floorSetsExceptLast?.SelectMany(x => x.Floors);
            var coffersTotalExceptLast = floorsExceptLast?.SelectMany(x => x.Coffers).GroupBy(x => x).Select(x => new StatisticsItem<Coffer>(x.Key, x.Count())).OrderByDescending(x => x.Value != Coffer.Potsherd && x.Value != Coffer.Medicine && x.Value != Coffer.Aetherpool).ThenBy(x => x.Value);
            var pomandersTotalExceptLast = floorsExceptLast?.SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).OrderBy(x => x.Value);

            this.Inventory = DataStatistics.BuildInventory(coffersTotalExceptLast, pomandersTotalExceptLast);

            if (this.IsEurekaOrthosFloor99)
                this.PomandersBossStatusTimer = this.FloorSet?.Floors.Where(x => x.Number == 99).SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).Where(x => DataStatistics.BossRelevantPomanders.Contains(x.Value)).Take(4);
            else
                this.PomandersBossStatusTimer = this.FloorSet?.LastFloor()?.Pomanders.GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count()))?.Where(x => DataStatistics.BossRelevantPomanders.Contains(x.Value)).Take(4);

            this.TimeLastFloor = new(this.FloorSet?.LastFloor()?.Time.Ticks ?? default);
            this.TimeTotal = new(this.FloorSet?.Floors.Sum(x => x.Time.Ticks) ?? default);

            this.ScoreLastFloor = this.FloorSet?.LastFloor()?.Score ?? 0;
            this.ScoreTotal = this.FloorSet?.Score() ?? 0;
        }
        else
        {
            var saveSlot = this.SaveSlotSummary ?? this.SaveSlot;

            this.FloorSets = saveSlot?.FloorSets;

            var floors = this.FloorSets?.SelectMany(x => x.Floors);
            var lastFloors = floors?.Where(x => x.IsLastFloor());

            this.MiscellaneousLastFloor = DataStatistics.GetMiscellaneousByFloors(lastFloors);
            this.MiscellaneousTotal = DataStatistics.GetMiscellaneousBySaveSlot(saveSlot);

            this.CoffersTotal = floors?.SelectMany(x => x.Coffers).GroupBy(x => x).Select(x => new StatisticsItem<Coffer>(x.Key, x.Count())).OrderByDescending(x => x.Value != Coffer.Potsherd && x.Value != Coffer.Medicine && x.Value != Coffer.Aetherpool).ThenBy(x => x.Value).Take(22);
            this.EnchantmentsTotal = floors?.SelectMany(x => x.Enchantments).GroupBy(x => x).Select(x => new StatisticsItem<Enchantment>(x.Key, x.Count()));
            this.TrapsTotal = floors?.SelectMany(x => x.Traps).GroupBy(x => x).Select(x => new StatisticsItem<Trap>(x.Key, x.Count()));

            this.PomandersLastFloor = lastFloors?.SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).Take(9);
            this.PomandersTotal = floors?.SelectMany(x => x.Pomanders).GroupBy(x => x).Select(x => new StatisticsItem<Pomander>(x.Key, x.Count())).OrderBy(x => x.Value).Take(22);

            this.Inventory = DataStatistics.BuildInventory(CoffersTotal, PomandersTotal);

            this.TimeLastFloor = new(lastFloors?.Sum(x => x.Time.Ticks) ?? default);
            this.TimeTotal = saveSlot?.Time() ?? default;

            this.ScoreLastFloor = lastFloors?.Sum(x => x.Score) ?? 0;
            this.ScoreTotal = saveSlot?.Score() ?? 0;
        }
    }

    private static IEnumerable<StatisticsItem<Pomander>>? BuildInventory(IEnumerable<StatisticsItem<Coffer>>? coffers, IEnumerable<StatisticsItem<Pomander>>? pomanders)
    {
        var coffersToPomanders = coffers?.Where(x => x.Value != Coffer.Potsherd && x.Value != Coffer.Medicine && x.Value != Coffer.Aetherpool).Select(x => new StatisticsItem<Pomander>((Pomander)x.Value, x.Total));
        return coffersToPomanders?.Select(x => new StatisticsItem<Pomander>(x.Value, x.Total - (pomanders?.FirstOrDefault(y => y.Value == x.Value)?.Total ?? 0))).Where(x => x.Total > 0);
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
            new (Miscellaneous.DreadBeast, floors?.Sum(x=> x.DreadBeasts) ?? 0),
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
                new (Miscellaneous.DreadBeast, floor?.DreadBeasts ?? 0),
                new (Miscellaneous.Death, floor?.Deaths ?? 0),
                new (Miscellaneous.RegenPotion, floor?.RegenPotions ?? 0),
                new (Miscellaneous.Map, Convert.ToInt32(floor?.Map, CultureInfo.InvariantCulture))
            }).RemoveAll(x => x.Total == 0 && x.Value != Miscellaneous.Map);
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
            new (Miscellaneous.DreadBeast, floorSet?.DreadBeasts() ?? 0),
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
            new (Miscellaneous.DreadBeast, saveSlot?.DreadBeasts() ?? 0),
            new (Miscellaneous.Death, saveSlot?.Deaths() ?? 0),
            new (Miscellaneous.RegenPotion, saveSlot?.RegenPotions() ?? 0),
            new (Miscellaneous.Map, saveSlot?.Maps() ?? 0),
            new (Miscellaneous.TimeBonus, saveSlot?.TimeBonuses() ?? 0)
        }).RemoveAll(x => x.Total == 0);
    }

    private static IEnumerable<StatisticsItem<BossStatusTimer>>? GetBossStatusTimerByFloorSet(FloorSet? floorSet)
    {
        var bst = floorSet?.BossStatusTimerData;
        var duration = (BossStatusTimerItem? item) => item?.Duration().TotalSeconds ?? 0;
        return ImmutableArray.CreateRange(new StatisticsItem<BossStatusTimer>[]
        {
            new (BossStatusTimer.Combat,(int) duration(bst?.Combat)),
            new (BossStatusTimer.Medicated,(int) (bst?.Medicated.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.DamageUp,(int) (bst?.DamageUp.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.DamageUpHeavenOnHigh,(int) (bst?.DamageUpHeavenOnHigh.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.DamageUpEurekaOrthos,(int) (bst?.DamageUpEurekaOrthos.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityDown,(int) (bst?.VulnerabilityDown.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityDownHeavenOnHigh,(int) (bst?.VulnerabilityDownHeavenOnHigh.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityDownEurekaOrthos,(int) (bst?.VulnerabilityDownEurekaOrthos.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.RehabilitationHeavenOnHigh,(int) (bst?.RehabilitationHeavenOnHigh.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.RehabilitationEurekaOrthos,(int) (bst?.RehabilitationEurekaOrthos.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityUp5,(int) (bst?.VulnerabilityUp.Where(x => x.Stacks == 5).Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityUp4,(int) (bst?.VulnerabilityUp.Where(x => x.Stacks == 4).Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityUp3,(int) (bst?.VulnerabilityUp.Where(x => x.Stacks == 3).Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityUp2,(int) (bst?.VulnerabilityUp.Where(x => x.Stacks == 2).Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.VulnerabilityUp,(int) (bst?.VulnerabilityUp.Where(x => x.Stacks == 1).Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.Enervation,(int) (bst?.Enervation.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.AccursedPox,(int) (bst?.AccursedPox.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.Weakness,(int) (bst?.Weakness.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),
            new (BossStatusTimer.BrinkOfDeath,(int) (bst?.BrinkOfDeath.Where(BossStatusTimerData.IsLongDuration).Sum(duration) ?? 0)),

        }).RemoveAll(x => x.Total == 0);
    }
}