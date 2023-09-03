using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker;

public class FloorSet
{
    [JsonInclude]
    public bool TimeBonus { get; private set; }

    [JsonInclude]
    public Collection<Floor> Floors { get; private set; } = new();

    [JsonInclude]
    public BossStatusTimerData? BossStatusTimerData { get; private set; }

    public TimeSpan Time() => new(this.Floors.Sum(x => x.Time.Ticks));

    public int Score() => this.Floors.Sum(x => x.Score);

    public int Kills() => this.Floors.Sum(x => x.Kills);

    public int CairnOfPassageKills() => this.Floors.Sum(x => x.CairnOfPassageKills);

    public int Mimics() => this.Floors.Sum(x => x.Mimics);

    public int Mandragoras() => this.Floors.Sum(x => x.Mandragoras);

    public int NPCs() => this.Floors.Sum(x => x.NPCs);

    public int DreadBeasts() => this.Floors.Sum(x => x.DreadBeasts);

    public int Coffers() => this.Floors.Sum(x => x.Coffers.Count);

    public int Enchantments() => this.Floors.Sum(x => x.Enchantments.Count);

    public int Traps() => this.Floors.Sum(x => x.Traps.Count);

    public int Pomanders() => this.Floors.Sum(x => x.Pomanders.Count);

    public int Deaths() => this.Floors.Sum(x => x.Deaths);

    public int RegenPotions() => this.Floors.Sum(x => x.RegenPotions);

    public int Potsherds() => this.Floors.Sum(x => x.Potsherds());

    public int Lurings() => this.Floors.Sum(x => x.Lurings());

    public int Maps() => this.Floors.Sum(x => x.Map ? 1 : 0);

    public Floor? FirstFloor() => this.Floors.FirstOrDefault();

    public Floor? CurrentFloor() => this.Floors.LastOrDefault();

    public Floor? LastFloor() => this.Floors.FirstOrDefault(x => x.IsLastFloor());

    public void AddFloor(int number) => this.Floors.Add(new(number));

    public void ClearFloors() => this.Floors.Clear();

    public void CheckForTimeBonus(TimeSpan totalTime) => this.TimeBonus = totalTime <= new TimeSpan(0, 30, 2);

    public void NoTimeBonus() => this.TimeBonus = false;

    public BossStatusTimerManager StartBossStatusTimer(Action isBossDeadAction)
    {
        this.BossStatusTimerData = new();
        return new BossStatusTimerManager(this.BossStatusTimerData, isBossDeadAction);
    }

    public void EndBossStatusTimer() => this.BossStatusTimerData?.TimerEnd();
}