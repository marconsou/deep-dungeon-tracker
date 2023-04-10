using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Linq;

namespace DeepDungeonTracker;

public sealed class BossStatusTimerManager : IDisposable
{
    private BossStatusTimerData BossStatusTimerData { get; }

    private ConditionEvent Medicated { get; } = new();

    private ConditionEvent AccursedPox { get; } = new();

    private ConditionEvent Weakness { get; } = new();

    private ConditionEvent BrinkOfDeath { get; } = new();

    private ConditionEvent DamageUp { get; } = new();

    private ConditionEvent VulnerabilityDown { get; } = new();

    private ConditionEvent VulnerabilityUp { get; } = new();

    private ConditionEvent Enervation { get; } = new();

    private ConditionEvent DamageUpEurekaOrthos { get; } = new();

    private ConditionEvent VulnerabilityDownEurekaOrthos { get; } = new();

    public BossStatusTimerManager(BossStatusTimerData bossStatusTimerData)
    {
        this.BossStatusTimerData = bossStatusTimerData;

        this.Medicated.AddActivating(this.MedicatedActivating);
        this.Medicated.AddDeactivating(this.MedicatedDeactivating);
        this.AccursedPox.AddActivating(this.AccursedPoxActivating);
        this.AccursedPox.AddDeactivating(this.AccursedPoxDeactivating);
        this.Weakness.AddActivating(this.WeaknessActivating);
        this.Weakness.AddDeactivating(this.WeaknessDeactivating);
        this.BrinkOfDeath.AddActivating(this.BrinkOfDeathActivating);
        this.BrinkOfDeath.AddDeactivating(this.BrinkOfDeathDeactivating);
        this.DamageUp.AddActivating(this.DamageUpActivating);
        this.DamageUp.AddDeactivating(this.DamageUpDeactivating);
        this.VulnerabilityDown.AddActivating(this.VulnerabilityDownActivating);
        this.VulnerabilityDown.AddDeactivating(this.VulnerabilityDownDeactivating);
        this.VulnerabilityUp.AddActivating(this.VulnerabilityUpActivating);
        this.VulnerabilityUp.AddDeactivating(this.VulnerabilityUpDeactivating);
        this.Enervation.AddActivating(this.EnervationActivating);
        this.Enervation.AddDeactivating(this.EnervationDeactivating);
        this.DamageUpEurekaOrthos.AddActivating(this.DamageUpEurekaOrthosActivating);
        this.DamageUpEurekaOrthos.AddDeactivating(this.DamageUpEurekaOrthosDeactivating);
        this.VulnerabilityDownEurekaOrthos.AddActivating(this.VulnerabilityDownEurekaOrthosActivating);
        this.VulnerabilityDownEurekaOrthos.AddDeactivating(this.VulnerabilityDownEurekaOrthosDeactivating);
    }

    public void Dispose()
    {
        this.Medicated.RemoveActivating(this.MedicatedActivating);
        this.Medicated.RemoveDeactivating(this.MedicatedDeactivating);
        this.AccursedPox.RemoveActivating(this.AccursedPoxActivating);
        this.AccursedPox.RemoveDeactivating(this.AccursedPoxDeactivating);
        this.Weakness.RemoveActivating(this.WeaknessActivating);
        this.Weakness.RemoveDeactivating(this.WeaknessDeactivating);
        this.BrinkOfDeath.RemoveActivating(this.BrinkOfDeathActivating);
        this.BrinkOfDeath.RemoveDeactivating(this.BrinkOfDeathDeactivating);
        this.DamageUp.RemoveActivating(this.DamageUpActivating);
        this.DamageUp.RemoveDeactivating(this.DamageUpDeactivating);
        this.VulnerabilityDown.RemoveActivating(this.VulnerabilityDownActivating);
        this.VulnerabilityDown.RemoveDeactivating(this.VulnerabilityDownDeactivating);
        this.VulnerabilityUp.RemoveActivating(this.VulnerabilityUpActivating);
        this.VulnerabilityUp.RemoveDeactivating(this.VulnerabilityUpDeactivating);
        this.Enervation.RemoveActivating(this.EnervationActivating);
        this.Enervation.RemoveDeactivating(this.EnervationDeactivating);
        this.DamageUpEurekaOrthos.RemoveActivating(this.DamageUpEurekaOrthosActivating);
        this.DamageUpEurekaOrthos.RemoveDeactivating(this.DamageUpEurekaOrthosDeactivating);
        this.VulnerabilityDownEurekaOrthos.RemoveActivating(this.VulnerabilityDownEurekaOrthosActivating);
        this.VulnerabilityDownEurekaOrthos.RemoveDeactivating(this.VulnerabilityDownEurekaOrthosDeactivating);
    }

    public void MedicatedActivating() => this.BossStatusTimerData?.Medicated?.Add(new(BossStatusTimer.Medicated));

    public void MedicatedDeactivating() => this.BossStatusTimerData?.Medicated?.LastOrDefault()?.TimerEnd();

    public void AccursedPoxActivating() => this.BossStatusTimerData?.AccursedPox?.Add(new(BossStatusTimer.AccursedPox));

    public void AccursedPoxDeactivating() => this.BossStatusTimerData?.AccursedPox?.LastOrDefault()?.TimerEnd();

    public void WeaknessActivating() => this.BossStatusTimerData?.Weakness?.Add(new(BossStatusTimer.Weakness));

    public void WeaknessDeactivating() => this.BossStatusTimerData?.Weakness?.LastOrDefault()?.TimerEnd();

    public void BrinkOfDeathActivating() => this.BossStatusTimerData?.BrinkOfDeath?.Add(new(BossStatusTimer.BrinkOfDeath));

    public void BrinkOfDeathDeactivating() => this.BossStatusTimerData?.BrinkOfDeath?.LastOrDefault()?.TimerEnd();

    public void DamageUpActivating() => this.BossStatusTimerData?.DamageUp?.Add(new(BossStatusTimer.DamageUp));

    public void DamageUpDeactivating() => this.BossStatusTimerData?.DamageUp?.LastOrDefault()?.TimerEnd();

    public void VulnerabilityDownActivating() => this.BossStatusTimerData?.VulnerabilityDown?.Add(new(BossStatusTimer.VulnerabilityDown));

    public void VulnerabilityDownDeactivating() => this.BossStatusTimerData?.VulnerabilityDown?.LastOrDefault()?.TimerEnd();

    public void VulnerabilityUpActivating() => this.BossStatusTimerData?.VulnerabilityUp?.Add(new(BossStatusTimer.VulnerabilityUp));

    public void VulnerabilityUpDeactivating() => this.BossStatusTimerData?.VulnerabilityUp?.LastOrDefault()?.TimerEnd();

    public void EnervationActivating() => this.BossStatusTimerData?.Enervation?.Add(new(BossStatusTimer.Enervation));

    public void EnervationDeactivating() => this.BossStatusTimerData?.Enervation?.LastOrDefault()?.TimerEnd();

    public void DamageUpEurekaOrthosActivating() => this.BossStatusTimerData?.DamageUpEurekaOrthos?.Add(new(BossStatusTimer.DamageUpEurekaOrthos));

    public void DamageUpEurekaOrthosDeactivating() => this.BossStatusTimerData?.DamageUpEurekaOrthos?.LastOrDefault()?.TimerEnd();

    public void VulnerabilityDownEurekaOrthosActivating() => this.BossStatusTimerData?.VulnerabilityDownEurekaOrthos?.Add(new(BossStatusTimer.VulnerabilityDownEurekaOrthos));

    public void VulnerabilityDownEurekaOrthosDeactivating() => this.BossStatusTimerData?.VulnerabilityDownEurekaOrthos?.LastOrDefault()?.TimerEnd();

    public void Update(BattleChara? enemy)
    {
        var player = Service.ClientState.LocalPlayer;
        if (player == null)
            return;

        this.BossStatusTimerData.Update(enemy);

        this.Medicated.Update(player.StatusList.Any(x => x.StatusId == 49));
        this.AccursedPox.Update(player.StatusList.Any(x => x.StatusId == 1087));
        this.Weakness.Update(player.StatusList.Any(x => x.StatusId == 43));
        this.BrinkOfDeath.Update(player.StatusList.Any(x => x.StatusId == 44));
        this.DamageUp.Update(player.StatusList.Any(x => x.StatusId == 687));
        this.VulnerabilityDown.Update(player.StatusList.Any(x => x.StatusId == 1100));

        if (enemy != null)
            this.VulnerabilityUp.Update(enemy.StatusList.Any(x => x.StatusId == 714));

        if (enemy != null)
            this.Enervation.Update(enemy.StatusList.Any(x => x.StatusId == 546));

        this.DamageUpEurekaOrthos.Update(player.StatusList.Any(x => x.StatusId == 3490));
        this.VulnerabilityDownEurekaOrthos.Update(player.StatusList.Any(x => x.StatusId == 3491));
    }

    public void ResetStatusState()
    {
        this.Medicated.Update(false);
        this.AccursedPox.Update(false);
        this.Weakness.Update(false);
        this.BrinkOfDeath.Update(false);
        this.DamageUp.Update(false);
        this.VulnerabilityDown.Update(false);
        this.VulnerabilityUp.Update(false);
        this.Enervation.Update(false);
        this.DamageUpEurekaOrthos.Update(false);
        this.VulnerabilityDownEurekaOrthos.Update(false);
    }
}