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
    public Collection<BossStatusTimerItem> Medicated { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("Medicated")]
    public Collection<BossStatusTimerItem>? SerializationMedicated { get => this.Medicated?.Count > 0 ? this.Medicated : null; private set => this.Medicated = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> AccursedPox { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("AccursedPox")]
    public Collection<BossStatusTimerItem>? SerializationAccursedPox { get => this.AccursedPox?.Count > 0 ? this.AccursedPox : null; private set => this.AccursedPox = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> Weakness { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("Weakness")]
    public Collection<BossStatusTimerItem>? SerializationWeakness { get => this.Weakness?.Count > 0 ? this.Weakness : null; private set => this.Weakness = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> BrinkOfDeath { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("BrinkOfDeath")]
    public Collection<BossStatusTimerItem>? SerializationBrinkOfDeath { get => this.BrinkOfDeath?.Count > 0 ? this.BrinkOfDeath : null; private set => this.BrinkOfDeath = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> DamageUp { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("DamageUp")]
    public Collection<BossStatusTimerItem>? SerializationDamageUp { get => this.DamageUp?.Count > 0 ? this.DamageUp : null; private set => this.DamageUp = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityDown { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("VulnerabilityDown")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityDown { get => this.VulnerabilityDown?.Count > 0 ? this.VulnerabilityDown : null; private set => this.VulnerabilityDown = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityUp { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("VulnerabilityUp")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityUp { get => this.VulnerabilityUp?.Count > 0 ? this.VulnerabilityUp : null; private set => this.VulnerabilityUp = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> Enervation { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("Enervation")]
    public Collection<BossStatusTimerItem>? SerializationEnervation { get => this.Enervation?.Count > 0 ? this.Enervation : null; private set => this.Enervation = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> DamageUpHeavenOnHigh { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("DamageUpHeavenOnHigh")]
    public Collection<BossStatusTimerItem>? SerializationDamageUpHeavenOnHigh { get => this.DamageUpHeavenOnHigh?.Count > 0 ? this.DamageUpHeavenOnHigh : null; private set => this.DamageUpHeavenOnHigh = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityDownHeavenOnHigh { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("VulnerabilityDownHeavenOnHigh")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityDownHeavenOnHigh { get => this.VulnerabilityDownHeavenOnHigh?.Count > 0 ? this.VulnerabilityDownHeavenOnHigh : null; private set => this.VulnerabilityDownHeavenOnHigh = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> RehabilitationHeavenOnHigh { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("RehabilitationHeavenOnHigh")]
    public Collection<BossStatusTimerItem>? SerializationRehabilitationHeavenOnHigh { get => this.RehabilitationHeavenOnHigh?.Count > 0 ? this.RehabilitationHeavenOnHigh : null; private set => this.RehabilitationHeavenOnHigh = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> DamageUpEurekaOrthos { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("DamageUpEurekaOrthos")]
    public Collection<BossStatusTimerItem>? SerializationDamageUpEurekaOrthos { get => this.DamageUpEurekaOrthos?.Count > 0 ? this.DamageUpEurekaOrthos : null; private set => this.DamageUpEurekaOrthos = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> VulnerabilityDownEurekaOrthos { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("VulnerabilityDownEurekaOrthos")]
    public Collection<BossStatusTimerItem>? SerializationVulnerabilityDownEurekaOrthos { get => this.VulnerabilityDownEurekaOrthos?.Count > 0 ? this.VulnerabilityDownEurekaOrthos : null; private set => this.VulnerabilityDownEurekaOrthos = value ?? []; }

    [JsonIgnore]
    public Collection<BossStatusTimerItem> RehabilitationEurekaOrthos { get; private set; } = [];

    [JsonInclude]
    [JsonPropertyName("RehabilitationEurekaOrthos")]
    public Collection<BossStatusTimerItem>? SerializationRehabilitationEurekaOrthos { get => this.RehabilitationEurekaOrthos?.Count > 0 ? this.RehabilitationEurekaOrthos : null; private set => this.RehabilitationEurekaOrthos = value ?? []; }

    public void Update(IBattleChara? enemy)
    {
        var vulnerabilityUp = this.VulnerabilityUp.LastOrDefault();
        if (vulnerabilityUp != null)
        {
            var stacks = enemy?.StatusList.FirstOrDefault(x => x.StatusId == 714)?.Param ?? 0;
            vulnerabilityUp.StacksUpdate((byte)Math.Max(vulnerabilityUp.Stacks, stacks));
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
            this.Medicated.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.AccursedPox.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.Weakness.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.BrinkOfDeath.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.DamageUp.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.VulnerabilityDown.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            (this.VulnerabilityUp?.Where(IsLongDuration).DistinctBy(x => x.Stacks).Count() ?? 0) +
            this.Enervation.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.DamageUpHeavenOnHigh.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.VulnerabilityDownHeavenOnHigh.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.RehabilitationHeavenOnHigh.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.DamageUpEurekaOrthos.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.VulnerabilityDownEurekaOrthos.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count() +
            this.RehabilitationEurekaOrthos.Where(IsLongDuration).DistinctBy(x => x.BossStatusTimer).Count();
    }

    public static Collection<BossStatusTimerItem> RemoveShortDuration(ICollection<BossStatusTimerItem> data) => new(data.Where(IsLongDuration).ToList());

    public static bool IsLongDuration(BossStatusTimerItem item) => item?.Duration() > TimeSpan.FromSeconds(10);
}