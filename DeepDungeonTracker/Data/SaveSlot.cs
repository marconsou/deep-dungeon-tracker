using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker
{
    public class SaveSlot
    {
        [JsonInclude]
        public int ContentId { get; private set; }

        [JsonInclude]
        public int CurrentLevel { get; private set; }

        [JsonInclude]
        public int AetherpoolArm { get; private set; }

        [JsonInclude]
        public int AetherpoolArmor { get; private set; }

        [JsonIgnore]
        int _KOs;

        [JsonInclude]
        public int KOs { get { return this._KOs; } private set { this._KOs = Math.Min(value, 99); } }

        public SaveSlot(int contentId = 0, int currentLevel = 0)
        {
            this.ContentId = contentId;
            this.CurrentLevel = currentLevel;
        }

        public void KOed() => this.KOs++;

        public int Kills() => this.FloorSets.Sum(x => x.Kills());

        public int Mimics() => this.FloorSets.Sum(x => x.Mimics());

        public int Mandragoras() => this.FloorSets.Sum(x => x.Mandragoras());

        public int NPCs() => this.FloorSets.Sum(x => x.NPCs());

        public int Coffers() => this.FloorSets.Sum(x => x.Coffers());

        public int Enchantments() => this.FloorSets.Sum(x => x.Enchantments());

        public int Traps() => this.FloorSets.Sum(x => x.Traps());

        public int Deaths() => this.FloorSets.Sum(x => x.Deaths());

        public int RegenPotions() => this.FloorSets.Sum(x => x.RegenPotions());

        public int Potsherds() => this.FloorSets.Sum(x => x.Potsherds());

        public int Lurings() => this.FloorSets.Sum(x => x.Lurings());

        public int Maps() => this.FloorSets.Sum(x => x.Maps());

        public int TimeBonuses() => this.FloorSets.Sum(x => x.TimeBonus ? 1 : 0);

        public TimeSpan Time() => new(this.FloorSets.Sum(x => x.Time().Ticks));

        public int Score() => this.FloorSets.Sum(x => x.Score());

        [JsonInclude]
        public List<FloorSet> FloorSets { get; private set; } = new();

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
            this.AetherpoolArm = arm;
            this.AetherpoolArmor = armor;
        }
    }
}