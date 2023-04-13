using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker;

public class BossStatusTimerData
{
    [JsonInclude]
    public BossStatusTimerItem Combat { get; private set; } = new(BossStatusTimer.Combat);

    [JsonIgnore]
    public Collection<BossStatusTimerItem> Medicated { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("Medicated")]
    public Collection<BossStatusTimerItem>? SerializationMedicated { get => this.Medicated?.Count > 0 ? this.Medicated : null; private set => this.Medicated = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> AccursedPox { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("AccursedPox")]
    public Collection<BossStatusTimerItem>? SerializationAccursedPox { get => this.AccursedPox?.Count > 0 ? this.AccursedPox : null; private set => this.AccursedPox = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> Weakness { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("Weakness")]
    public Collection<BossStatusTimerItem>? SerializationWeakness { get => this.Weakness?.Count > 0 ? this.Weakness : null; private set => this.Weakness = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> BrinkOfDeath { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("BrinkOfDeath")]
    public Collection<BossStatusTimerItem>? SerializationBrinkOfDeath { get => this.BrinkOfDeath?.Count > 0 ? this.BrinkOfDeath : null; private set => this.BrinkOfDeath = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> DamageUp { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("DamageUp")]
    public Collection<BossStatusTimerItem>? SerializationDamageUp { get => this.DamageUp?.Count > 0 ? this.DamageUp : null; private set => this.DamageUp = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityDown { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("VulnerabilityDown")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityDown { get => this.VulnerabilityDown?.Count > 0 ? this.VulnerabilityDown : null; private set => this.VulnerabilityDown = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityUp { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("VulnerabilityUp")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityUp { get => this.VulnerabilityUp?.Count > 0 ? this.VulnerabilityUp : null; private set => this.VulnerabilityUp = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> Enervation { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("Enervation")]
    public Collection<BossStatusTimerItem>? SerializationEnervation { get => this.Enervation?.Count > 0 ? this.Enervation : null; private set => this.Enervation = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> DamageUpHeavenOnHigh { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("DamageUpHeavenOnHigh")]
    public Collection<BossStatusTimerItem>? SerializationDamageUpHeavenOnHigh { get => this.DamageUpHeavenOnHigh?.Count > 0 ? this.DamageUpHeavenOnHigh : null; private set => this.DamageUpHeavenOnHigh = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityDownHeavenOnHigh { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("VulnerabilityDownHeavenOnHigh")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityDownHeavenOnHigh { get => this.VulnerabilityDownHeavenOnHigh?.Count > 0 ? this.VulnerabilityDownHeavenOnHigh : null; private set => this.VulnerabilityDownHeavenOnHigh = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> RehabilitationHeavenOnHigh { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("RehabilitationHeavenOnHigh")]
    public Collection<BossStatusTimerItem>? SerializationRehabilitationHeavenOnHigh { get => this.RehabilitationHeavenOnHigh?.Count > 0 ? this.RehabilitationHeavenOnHigh : null; private set => this.RehabilitationHeavenOnHigh = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> DamageUpEurekaOrthos { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("DamageUpEurekaOrthos")]
    public Collection<BossStatusTimerItem>? SerializationDamageUpEurekaOrthos { get => this.DamageUpEurekaOrthos?.Count > 0 ? this.DamageUpEurekaOrthos : null; private set => this.DamageUpEurekaOrthos = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityDownEurekaOrthos { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("VulnerabilityDownEurekaOrthos")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityDownEurekaOrthos { get => this.VulnerabilityDownEurekaOrthos?.Count > 0 ? this.VulnerabilityDownEurekaOrthos : null; private set => this.VulnerabilityDownEurekaOrthos = value ?? new(); }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> RehabilitationEurekaOrthos { get; private set; } = new();

    [JsonInclude]
    [JsonPropertyName("RehabilitationEurekaOrthos")]
    public Collection<BossStatusTimerItem>? SerializationRehabilitationEurekaOrthos { get => this.RehabilitationEurekaOrthos?.Count > 0 ? this.RehabilitationEurekaOrthos : null; private set => this.RehabilitationEurekaOrthos = value ?? new(); }

    public void Update(BattleChara? enemy)
    {
        var vulnerabilityUp = this.VulnerabilityUp.LastOrDefault();
        if (vulnerabilityUp != null)
        {
            var stacks = enemy?.StatusList.FirstOrDefault(x => x.StatusId == 714)?.StackCount ?? 0;
            vulnerabilityUp.StacksUpdate(Math.Max(vulnerabilityUp.Stacks, stacks));
        }
    }

    public void TimerEnd()
    {
        foreach (var item in this.Medicated)
            item.TimerEnd();

        foreach (var item in this.AccursedPox)
            item.TimerEnd();

        foreach (var item in this.Weakness)
            item.TimerEnd();

        foreach (var item in this.BrinkOfDeath)
            item.TimerEnd();

        foreach (var item in this.DamageUp)
            item.TimerEnd();

        foreach (var item in this.VulnerabilityDown)
            item.TimerEnd();

        foreach (var item in this.VulnerabilityUp)
            item.TimerEnd();

        foreach (var item in this.Enervation)
            item.TimerEnd();

        foreach (var item in this.DamageUpHeavenOnHigh)
            item.TimerEnd();

        foreach (var item in this.VulnerabilityDownHeavenOnHigh)
            item.TimerEnd();

        foreach (var item in this.RehabilitationHeavenOnHigh)
            item.TimerEnd();

        foreach (var item in this.DamageUpEurekaOrthos)
            item.TimerEnd();

        foreach (var item in this.VulnerabilityDownEurekaOrthos)
            item.TimerEnd();

        foreach (var item in this.RehabilitationEurekaOrthos)
            item.TimerEnd();

        this.Combat.TimerEnd();
    }

    public int TimersCount()
    {
        return 1 +
            this.Medicated.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.AccursedPox.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.Weakness.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.BrinkOfDeath.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.DamageUp.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.VulnerabilityDown.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            (this.VulnerabilityUp?.Where(DurationMoreThanOneSecond).DistinctBy(x => x.Stacks).Count() ?? 0) +
            this.Enervation.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.DamageUpHeavenOnHigh.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.VulnerabilityDownHeavenOnHigh.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.RehabilitationHeavenOnHigh.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.DamageUpEurekaOrthos.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.VulnerabilityDownEurekaOrthos.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count() +
            this.RehabilitationEurekaOrthos.Where(DurationMoreThanOneSecond).DistinctBy(x => x.BossStatusTimer).Count();
    }

    public static Collection<BossStatusTimerItem> RemoveLessThanOneSecondDuration(ICollection<BossStatusTimerItem> data)
    {
        var newData = new Collection<BossStatusTimerItem>();
        foreach (var item in data ?? Enumerable.Empty<BossStatusTimerItem>())
        {
            if (DurationMoreThanOneSecond(item))
                newData.Add(item);
        }
        return newData;
    }

    private static bool DurationMoreThanOneSecond(BossStatusTimerItem item) => item.Duration() > new TimeSpan(0, 0, 1);
}