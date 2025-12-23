using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Linq;

namespace DeepDungeonTracker;

public sealed class BossStatusTimerManager : IDisposable
{
    private BossStatusTimerData BossStatusTimerData { get; }

    private Action IsBossDeadAction { get; }

    private ConditionEvent IsBossDead { get; } = new();

    private ConditionEvent Medicated { get; } = new();

    private ConditionEvent AccursedPox { get; } = new();

    private ConditionEvent Weakness { get; } = new();

    private ConditionEvent BrinkOfDeath { get; } = new();

    private ConditionEvent DamageUp { get; } = new();

    private ConditionEvent VulnerabilityDown { get; } = new();

    private ConditionEvent VulnerabilityUp { get; } = new();

    private ConditionEvent Enervation { get; } = new();

    private ConditionEvent DamageUpHeavenOnHigh { get; } = new();

    private ConditionEvent VulnerabilityDownHeavenOnHigh { get; } = new();

    private ConditionEvent RehabilitationHeavenOnHigh { get; } = new();

    private ConditionEvent DamageUpEurekaOrthos { get; } = new();

    private ConditionEvent VulnerabilityDownEurekaOrthos { get; } = new();

    private ConditionEvent RehabilitationEurekaOrthos { get; } = new();

    public BossStatusTimerManager(BossStatusTimerData bossStatusTimerData, Action isBossDeadAction)
    {
        this.BossStatusTimerData = bossStatusTimerData;
        this.IsBossDeadAction = isBossDeadAction;

        this.IsBossDead.AddActivating(this.IsBossDeadAction);

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
        this.DamageUpHeavenOnHigh.AddActivating(this.DamageUpHeavenOnHighActivating);
        this.DamageUpHeavenOnHigh.AddDeactivating(this.DamageUpHeavenOnHighDeactivating);
        this.VulnerabilityDownHeavenOnHigh.AddActivating(this.VulnerabilityDownHeavenOnHighActivating);
        this.VulnerabilityDownHeavenOnHigh.AddDeactivating(this.VulnerabilityDownHeavenOnHighDeactivating);
        this.RehabilitationHeavenOnHigh.AddActivating(this.RehabilitationHeavenOnHighActivating);
        this.RehabilitationHeavenOnHigh.AddDeactivating(this.RehabilitationHeavenOnHighDeactivating);
        this.DamageUpEurekaOrthos.AddActivating(this.DamageUpEurekaOrthosActivating);
        this.DamageUpEurekaOrthos.AddDeactivating(this.DamageUpEurekaOrthosDeactivating);
        this.VulnerabilityDownEurekaOrthos.AddActivating(this.VulnerabilityDownEurekaOrthosActivating);
        this.VulnerabilityDownEurekaOrthos.AddDeactivating(this.VulnerabilityDownEurekaOrthosDeactivating);
        this.RehabilitationEurekaOrthos.AddActivating(this.RehabilitationEurekaOrthosActivating);
        this.RehabilitationEurekaOrthos.AddDeactivating(this.RehabilitationEurekaOrthosDeactivating);
    }

    public void Dispose()
    {
        this.IsBossDead.RemoveActivating(this.IsBossDeadAction);

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
        this.DamageUpHeavenOnHigh.RemoveActivating(this.DamageUpHeavenOnHighActivating);
        this.DamageUpHeavenOnHigh.RemoveDeactivating(this.DamageUpHeavenOnHighDeactivating);
        this.VulnerabilityDownHeavenOnHigh.RemoveActivating(this.VulnerabilityDownHeavenOnHighActivating);
        this.VulnerabilityDownHeavenOnHigh.RemoveDeactivating(this.VulnerabilityDownHeavenOnHighDeactivating);
        this.RehabilitationHeavenOnHigh.RemoveActivating(this.RehabilitationHeavenOnHighActivating);
        this.RehabilitationHeavenOnHigh.RemoveDeactivating(this.RehabilitationHeavenOnHighDeactivating);
        this.DamageUpEurekaOrthos.RemoveActivating(this.DamageUpEurekaOrthosActivating);
        this.DamageUpEurekaOrthos.RemoveDeactivating(this.DamageUpEurekaOrthosDeactivating);
        this.VulnerabilityDownEurekaOrthos.RemoveActivating(this.VulnerabilityDownEurekaOrthosActivating);
        this.VulnerabilityDownEurekaOrthos.RemoveDeactivating(this.VulnerabilityDownEurekaOrthosDeactivating);
        this.RehabilitationEurekaOrthos.RemoveActivating(this.RehabilitationEurekaOrthosActivating);
        this.RehabilitationEurekaOrthos.RemoveDeactivating(this.RehabilitationEurekaOrthosDeactivating);
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

    public void DamageUpHeavenOnHighActivating() => this.BossStatusTimerData?.DamageUpHeavenOnHigh?.Add(new(BossStatusTimer.DamageUpHeavenOnHigh));

    public void DamageUpHeavenOnHighDeactivating() => this.BossStatusTimerData?.DamageUpHeavenOnHigh?.LastOrDefault()?.TimerEnd();

    public void VulnerabilityDownHeavenOnHighActivating() => this.BossStatusTimerData?.VulnerabilityDownHeavenOnHigh?.Add(new(BossStatusTimer.VulnerabilityDownHeavenOnHigh));

    public void VulnerabilityDownHeavenOnHighDeactivating() => this.BossStatusTimerData?.VulnerabilityDownHeavenOnHigh?.LastOrDefault()?.TimerEnd();

    public void RehabilitationHeavenOnHighActivating() => this.BossStatusTimerData?.RehabilitationHeavenOnHigh?.Add(new(BossStatusTimer.RehabilitationHeavenOnHigh));

    public void RehabilitationHeavenOnHighDeactivating() => this.BossStatusTimerData?.RehabilitationHeavenOnHigh?.LastOrDefault()?.TimerEnd();

    public void DamageUpEurekaOrthosActivating() => this.BossStatusTimerData?.DamageUpEurekaOrthos?.Add(new(BossStatusTimer.DamageUpEurekaOrthos));

    public void DamageUpEurekaOrthosDeactivating() => this.BossStatusTimerData?.DamageUpEurekaOrthos?.LastOrDefault()?.TimerEnd();

    public void VulnerabilityDownEurekaOrthosActivating() => this.BossStatusTimerData?.VulnerabilityDownEurekaOrthos?.Add(new(BossStatusTimer.VulnerabilityDownEurekaOrthos));

    public void VulnerabilityDownEurekaOrthosDeactivating() => this.BossStatusTimerData?.VulnerabilityDownEurekaOrthos?.LastOrDefault()?.TimerEnd();

    public void RehabilitationEurekaOrthosActivating() => this.BossStatusTimerData?.RehabilitationEurekaOrthos?.Add(new(BossStatusTimer.RehabilitationEurekaOrthos));

    public void RehabilitationEurekaOrthosDeactivating() => this.BossStatusTimerData?.RehabilitationEurekaOrthos?.LastOrDefault()?.TimerEnd();

    public void Update(IBattleChara? enemy)
    {
        var player = Service.ObjectTable.LocalPlayer;
        if (player == null)
            return;

        this.BossStatusTimerData.Update(enemy);

        this.IsBossDead.Update(enemy?.IsDead ?? false);

        this.Medicated.Update(player.StatusList.Any(x => x.StatusId == 49));
        this.AccursedPox.Update(player.StatusList.Any(x => x.StatusId == 1087));
        this.Weakness.Update(player.StatusList.Any(x => x.StatusId == 43));
        this.BrinkOfDeath.Update(player.StatusList.Any(x => x.StatusId == 44));
        this.DamageUp.Update(player.StatusList.Any(x => x.StatusId == 687));
        this.VulnerabilityDown.Update(player.StatusList.Any(x => x.StatusId == 1100));
        this.VulnerabilityUp.Update(enemy?.StatusList.Any(x => x.StatusId == 714) ?? false);
        this.Enervation.Update(enemy?.StatusList.Any(x => x.StatusId == 546) ?? false);
        this.DamageUpHeavenOnHigh.Update(player.StatusList.Any(x => x.StatusId == 1584));
        this.VulnerabilityDownHeavenOnHigh.Update(player.StatusList.Any(x => x.StatusId == 1585));
        this.RehabilitationHeavenOnHigh.Update(player.StatusList.Any(x => x.StatusId == 1586));
        this.DamageUpEurekaOrthos.Update(player.StatusList.Any(x => x.StatusId == 3490));
        this.VulnerabilityDownEurekaOrthos.Update(player.StatusList.Any(x => x.StatusId == 3491));
        this.RehabilitationEurekaOrthos.Update(player.StatusList.Any(x => x.StatusId == 3492));
    }
}