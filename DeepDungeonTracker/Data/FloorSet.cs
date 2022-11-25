using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker
{
    public class FloorSet
    {
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool TimeBonus { get; private set; }

        [JsonInclude]
        public List<Floor> Floors { get; private set; } = new();

        public int Kills() => this.Floors.Sum(x => x.Kills);

        public int Mimics() => this.Floors.Sum(x => x.Mimics);

        public int Mandragoras() => this.Floors.Sum(x => x.Mandragoras);

        public int NPCs() => this.Floors.Sum(x => x.NPCs);

        public int Coffers() => this.Floors.Sum(x => x.Coffers.Count);

        public int Enchantments() => this.Floors.Sum(x => x.Enchantments.Count);

        public int Traps() => this.Floors.Sum(x => x.Traps.Count);

        public int Deaths() => this.Floors.Sum(x => x.Deaths);

        public int RegenPotions() => this.Floors.Sum(x => x.RegenPotions);

        public int Potsherds() => this.Floors.Sum(x => x.Potsherds());

        public int Lurings() => this.Floors.Sum(x => x.Lurings());

        public int Maps() => this.Floors.Sum(x => x.Map ? 1 : 0);

        public int Score() => this.Floors.Sum(x => x.Score);

        public Floor? FirstFloor() => this.Floors.FirstOrDefault();

        public Floor? CurrentFloor() => this.Floors.LastOrDefault();

        public Floor? LastFloor() => this.Floors.Find(x => x.IsLastFloor());

        public void AddFloor(int number) => this.Floors.Add(new(number));

        public void ClearFloors() => this.Floors.Clear();

        public void CheckForTimeBonus(TimeSpan totalTime) => this.TimeBonus = totalTime <= new TimeSpan(0, 30, 0);

        public void NoTimeBonus() => this.TimeBonus = false;
    }
}