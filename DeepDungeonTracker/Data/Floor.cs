using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker;

public class Floor
{
    [JsonInclude]
    public int Number { get; private set; }

    [JsonInclude]
    public TimeSpan Time { get; private set; } = new();

    [JsonInclude]
    public int Score { get; private set; }

    [JsonInclude]
    public int Kills { get; private set; }

    [JsonInclude]
    public int CairnOfPassageKills { get; private set; }

    [JsonInclude]
    public int Mimics { get; private set; }

    [JsonInclude]
    public int Mandragoras { get; private set; }

    [JsonInclude]
    public int NPCs { get; private set; }

    [JsonInclude]
    public int DreadBeasts { get; private set; }

    [JsonIgnore]
    public Collection<Coffer> Coffers { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("Coffers")]
    public Collection<Coffer>? SerializationCoffers { get => this.Coffers?.Count > 0 ? this.Coffers : null; private set => this.Coffers = value ?? new(); }

    [JsonIgnore]
    public Collection<Enchantment> Enchantments { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("Enchantments")]
    public Collection<Enchantment>? SerializationEnchantments { get => this.Enchantments?.Count > 0 ? this.Enchantments : null; private set => this.Enchantments = value ?? new(); }

    [JsonIgnore]
    public Collection<Enchantment> EnchantmentsSerenized { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("EnchantmentsSerenized")]
    public Collection<Enchantment>? SerializationEnchantmentsSerenized { get => this.EnchantmentsSerenized?.Count > 0 ? this.EnchantmentsSerenized : null; private set => this.EnchantmentsSerenized = value ?? new(); }

    [JsonIgnore]
    public Collection<Trap> Traps { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("Traps")]
    public Collection<Trap>? SerializationTraps { get => this.Traps?.Count > 0 ? this.Traps : null; private set => this.Traps = value ?? new(); }

    [JsonIgnore]
    public Collection<Pomander> Pomanders { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("Pomanders")]
    public Collection<Pomander>? SerializationPomanders { get => this.Pomanders?.Count > 0 ? this.Pomanders : null; private set => this.Pomanders = value ?? new(); }

    [JsonInclude]
    public int Deaths { get; private set; }

    [JsonInclude]
    public int RegenPotions { get; private set; }

    [JsonInclude]
    public bool Map { get; private set; }

    [JsonInclude]
    public MapData MapData { get; private set; } = new();

    public Floor(int number) => this.Number = number;

    public bool IsFirstFloor() => this.Number % 10 == 1;

    public bool IsLastFloor() => this.Number % 10 == 0;

    public void TimeUpdate(TimeSpan time) => this.Time = time;

    public void ScoreUpdate(int score) => this.Score = score;

    public void EnemyKilled() => this.Kills++;

    public void CairnOfPassageShines() => this.CairnOfPassageKills++;

    public void MimicKilled() => this.Mimics++;

    public void MandragoraKilled() => this.Mandragoras++;

    public void NPCKilled() => this.NPCs++;

    public void DreadBeastKilled() => this.DreadBeasts++;

    public void CofferOpened(Coffer coffer) => this.Coffers.Add(coffer);

    public void EnchantmentAffected(Enchantment enchantment) => this.Enchantments.Add(enchantment);

    public void TrapTriggered(Trap trap) => this.Traps.Add(trap);

    public void PomanderUsed(Pomander pomander)
    {
        this.Pomanders.Add(pomander);
        if (pomander == Pomander.Serenity)
        {
            if (this.Enchantments.Count > 0)
            {
                this.EnchantmentsSerenized.Clear();
                foreach (var item in this.Enchantments)
                    this.EnchantmentsSerenized.Add(item);
                this.Enchantments.Clear();
            }
        }
    }

    public void PlayerKilled() => this.Deaths++;

    public void RegenPotionConsumed() => this.RegenPotions++;

    public void MapFullyRevealed() => this.Map = true;

    public int Potsherds() => this.Coffers.Count(x => x == Coffer.Potsherd);

    public int Lurings() => this.Traps.Count(x => x == Trap.Luring);

    public IEnumerable<Enchantment> AdjustedEnchantments() => this.Enchantments.Count > 0 ? this.Enchantments : this.EnchantmentsSerenized.Count > 0 ? this.EnchantmentsSerenized : new();
}