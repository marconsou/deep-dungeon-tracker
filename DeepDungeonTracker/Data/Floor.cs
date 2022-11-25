using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker
{
    public class Floor
    {
        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Number { get; private set; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Kills { get; private set; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Mimics { get; private set; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Mandragoras { get; private set; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int NPCs { get; private set; }

        [JsonIgnore]
        public List<Coffer> Coffers { get; private set; } = new();

        [JsonInclude]
        [JsonPropertyName("Coffers")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Coffer>? SerializationCoffers { get => this.Coffers?.Count > 0 ? this.Coffers : null; private set => this.Coffers = value ?? new(); }

        [JsonIgnore]
        public List<Enchantment> Enchantments { get; private set; } = new();

        [JsonInclude]
        [JsonPropertyName("Enchantments")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Enchantment>? SerializationEnchantments { get => this.Enchantments?.Count > 0 ? this.Enchantments : null; private set => this.Enchantments = value ?? new(); }

        [JsonIgnore]
        public List<Trap> Traps { get; private set; } = new();

        [JsonInclude]
        [JsonPropertyName("Traps")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Trap>? SerializationTraps { get => this.Traps?.Count > 0 ? this.Traps : null; private set => this.Traps = value ?? new(); }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Deaths { get; private set; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int RegenPotions { get; private set; }

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public bool Map { get; private set; }

        [JsonInclude]
        public MapData MapData { get; private set; } = new();

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public TimeSpan Time { get; private set; } = new();

        [JsonInclude]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int Score { get; private set; } = new();

        public Floor(int number) => this.Number = number;

        public bool IsFirstFloor() => this.Number % 10 == 1;

        public bool IsLastFloor() => this.Number % 10 == 0;

        public void EnemyKilled() => this.Kills++;

        public void MimicKilled() => this.Mimics++;

        public void MandragoraKilled() => this.Mandragoras++;

        public void NPCKilled() => this.NPCs++;

        public void PlayerKilled() => this.Deaths++;

        public void RegenPotionConsumed() => this.RegenPotions++;

        public void MapFullyRevealed() => this.Map = true;

        public void TimeUpdate(TimeSpan time) => this.Time = time;

        public void ScoreUpdate(int score) => this.Score = score;

        public int Potsherds() => this.Coffers.Count(x => x == Coffer.Potsherd);

        public int Lurings() => this.Traps.Count(x => x == Trap.Luring);
    }
}