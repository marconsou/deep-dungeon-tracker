using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker;

public class SaveSlot
{
    [JsonInclude]
    public DeepDungeon DeepDungeon { get; private set; }

    [JsonInclude]
    public int ContentId { get; private set; }

    [JsonInclude]
    public uint ClassJobId { get; private set; }

    [JsonInclude]
    public int CurrentLevel { get; private set; }

    [JsonInclude]
    public int AetherpoolArm { get; private set; }

    [JsonInclude]
    public int AetherpoolArmor { get; private set; }

    [JsonIgnore]
    private int _KOs;

    [JsonInclude]
    public int KOs { get { return this._KOs; } private set { this._KOs = Math.Min(value, 99); } }

    [JsonInclude]
    public Collection<FloorSet> FloorSets { get; private set; } = new();

    public SaveSlot(DeepDungeon deepDungeon = DeepDungeon.None, int contentId = 0, uint classJobId = 0, int currentLevel = 0)
    {
        this.DeepDungeon = deepDungeon;
        this.ContentId = contentId;
        this.ClassJobId = classJobId;
        this.CurrentLevel = currentLevel;
    }

    public void KOed() => this.KOs++;

    public TimeSpan Time() => new(this.FloorSets.Sum(x => x.Time().Ticks));

    public int Score() => this.FloorSets.Sum(x => x.Score());

    public int Kills() => this.FloorSets.Sum(x => x.Kills());

    public int CairnOfPassageKills() => this.FloorSets.Sum(x => x.CairnOfPassageKills());

    public int Mimics() => this.FloorSets.Sum(x => x.Mimics());

    public int Mandragoras() => this.FloorSets.Sum(x => x.Mandragoras());

    public int NPCs() => this.FloorSets.Sum(x => x.NPCs());

    public int DreadBeasts() => this.FloorSets.Sum(x => x.DreadBeasts());

    public int Coffers() => this.FloorSets.Sum(x => x.Coffers());

    public int Enchantments() => this.FloorSets.Sum(x => x.Enchantments());

    public int Traps() => this.FloorSets.Sum(x => x.Traps());

    public int Pomanders() => this.FloorSets.Sum(x => x.Pomanders());

    public int Deaths() => this.FloorSets.Sum(x => x.Deaths());

    public int RegenPotions() => this.FloorSets.Sum(x => x.RegenPotions());

    public int Potsherds() => this.FloorSets.Sum(x => x.Potsherds());

    public int Lurings() => this.FloorSets.Sum(x => x.Lurings());

    public int Maps() => this.FloorSets.Sum(x => x.Maps());

    public int HallOfFallacies() => this.FloorSets.Sum(x => x.HallOfFallacies());

    public int TimeBonuses() => this.FloorSets.Sum(x => x.TimeBonus ? 1 : 0);

    public FloorSet? CurrentFloorSet() => this.FloorSets.LastOrDefault();

    public Floor? CurrentFloor() => this.CurrentFloorSet()?.CurrentFloor();

    public void AddFloor() => this.CurrentFloorSet()?.AddFloor(this.CurrentFloorNumber() + 1);

    public void AddFloorSet(int floorNumber)
    {
        var floorSet = new FloorSet();
        this.FloorSets.Add(floorSet);
        floorSet.AddFloor(floorNumber);
    }

    public void ResetFloorSet()
    {
        if (this.FloorSets.Count == 0)
            return;

        var floorSet = this.CurrentFloorSet();
        var firstFloorNumber = floorSet?.FirstFloor()?.Number ?? 0;
        floorSet?.ClearFloors();
        floorSet?.AddFloor(firstFloorNumber);
    }

    public void ContentIdUpdate(int contentId) => this.ContentId = contentId;

    public int StartingFloorNumber() => this.FloorSets?.FirstOrDefault()?.FirstFloor()?.Number ?? 0;

    public int CurrentFloorNumber() => this.CurrentFloor()?.Number ?? 0;

    public void CurrentLevelUpdate() => this.CurrentLevel = Service.ClientState.LocalPlayer?.Level ?? 0;

    public void AetherpoolUpdate(int arm, int armor)
    {
        this.AetherpoolArm = Math.Max(this.AetherpoolArm, arm);
        this.AetherpoolArmor = Math.Max(this.AetherpoolArmor, armor);
    }

    public bool IsSpecialBossFloor(Floor? floor) => (this.DeepDungeon == DeepDungeon.EurekaOrthos && floor?.Number == 99);

    public void AdditionalKills(int flootSetIndex, int kills)
    {
        if (kills <= 0)
            return;

        var flootSet = flootSetIndex < this.FloorSets.Count ? this.FloorSets[flootSetIndex] : null;
        if (flootSet == null)
            return;

        var firstFloor = flootSet.FirstFloor();
        for (var i = 0; i < kills; i++)
            firstFloor?.EnemyKilled();
    }

    public void AdditionalMimicKills(int flootSetIndex, int kills)
    {
        if (kills <= 0)
            return;

        var flootSet = flootSetIndex < this.FloorSets.Count ? this.FloorSets[flootSetIndex] : null;
        if (flootSet == null)
            return;

        var firstFloor = flootSet.FirstFloor();
        for (var i = 0; i < kills; i++)
            firstFloor?.MimicKilled();
    }

    static public void Copy(SaveSlot? source, SaveSlot? dest, int maxFloor)
    {
        if (source == null || dest == null)
            return;

        dest.DeepDungeon = source.DeepDungeon;
        dest.ContentId = source.ContentId;
        dest.ClassJobId = source.ClassJobId;
        dest.CurrentLevel = source.CurrentLevel;
        dest.AetherpoolArm = source.AetherpoolArm;
        dest.AetherpoolArmor = source.AetherpoolArmor;
        dest.KOs = source.KOs;
        dest.FloorSets = new(source.FloorSets.ToList().Where(x => x.FirstFloor()?.Number <= maxFloor).ToList());
    }
}