using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Interface;
using DeepDungeonTracker.Event;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace DeepDungeonTracker;

public sealed class Data : IDisposable
{
    public DataCommon Common { get; } = new();

    public DataUI UI { get; }

    public DataAudio Audio { get; } = new();

    private DataText Text { get; } = new();

    public DataStatistics Statistics { get; } = new();

    private ConditionEvent BetweenAreas { get; } = new();

    private ConditionEvent InDutyQueue { get; } = new();

    private ConditionEvent InDeepDungeon { get; } = new();

    private ConditionEvent Occupied33 { get; } = new();

    private ConditionEvent InCombat { get; } = new();

    private Event<string> EnchantmentMessage { get; } = new();

    private Event<string> TrapMessage { get; } = new();

    private bool IsCharacterBusy => this.BetweenAreas.IsActivated || this.Occupied33.IsActivated || this.Common.IsTransferenceInitiated;

    public bool IsInsideDeepDungeon => this.InDeepDungeon.IsActivated;

    public bool IsInDeepDungeonSubArea =>
        this.Text.IsPalaceOfTheDeadRegion(Service.ClientState.TerritoryType, true) ||
        this.Text.IsHeavenOnHighRegion(Service.ClientState.TerritoryType, true) ||
        this.Text.IsEurekaOrthosRegion(Service.ClientState.TerritoryType, true);

    public Data(IUiBuilder uiBuilder, Configuration configuration)
    {
        this.UI = new(uiBuilder);

        this.InDeepDungeon.AddActivating(this.DeepDungeonActivating);
        this.InDeepDungeon.AddDeactivating(this.DeepDungeonDeactivating);
        this.InCombat.AddActivating(this.CombatActivating);
        this.InCombat.AddDeactivating(this.CombatDeactivating);
        this.EnchantmentMessage.Add(this.EnchantmentMessageReceived);
        this.TrapMessage.Add(this.TrapMessageReceived);
        ItemChangedEvents<PomanderChangedType>.Changed += this.PomanderChangedAction;
        ItemChangedEvents<StoneChangedType>.Changed += this.StoneChangedAction;
        AetherpoolObtainedEvents.Changed += this.AetherpoolObtainedAction;
        NewFloorEvents.Changed += this.FloorChangeAction;
        CharacterKilledEvents.Changed += this.CharacterKilledAction;
        RegenPotionConsumedEvents.Changed += this.RegenPotionConsumedAction;
        TransferenceInitiatedEvents.Changed += this.TransferenceInitiatedAction;
        DutyFailedEvents.Changed += this.DutyFailedAction;

        if (Service.ClientState.IsLoggedIn)
        {
            this.ConditionChange(ConditionFlag.BetweenAreas, Service.Condition[ConditionFlag.BetweenAreas]);
            this.ConditionChange(ConditionFlag.InDutyQueue, Service.Condition[ConditionFlag.InDutyQueue]);
            this.ConditionChange(ConditionFlag.InDeepDungeon, Service.Condition[ConditionFlag.InDeepDungeon]);
            this.ConditionChange(ConditionFlag.Occupied33, Service.Condition[ConditionFlag.Occupied33]);
            this.ConditionChange(ConditionFlag.InCombat, Service.Condition[ConditionFlag.InCombat]);
            this.TerritoryChanged(Service.ClientState.TerritoryType);
            this.Login();
        }
    }

    public void Dispose()
    {
        this.InDeepDungeon.RemoveActivating(this.DeepDungeonActivating);
        this.InDeepDungeon.RemoveDeactivating(this.DeepDungeonDeactivating);
        this.InCombat.RemoveActivating(this.CombatActivating);
        this.InCombat.RemoveDeactivating(this.CombatDeactivating);
        this.EnchantmentMessage.Remove(this.EnchantmentMessageReceived);
        this.TrapMessage.Remove(this.TrapMessageReceived);
        ItemChangedEvents<PomanderChangedType>.Changed -= this.PomanderChangedAction;
        ItemChangedEvents<StoneChangedType>.Changed -= this.StoneChangedAction;
        AetherpoolObtainedEvents.Changed -= this.AetherpoolObtainedAction;
        NewFloorEvents.Changed -= this.FloorChangeAction;
        CharacterKilledEvents.Changed -= this.CharacterKilledAction;
        RegenPotionConsumedEvents.Changed -= this.RegenPotionConsumedAction;
        TransferenceInitiatedEvents.Changed -= this.TransferenceInitiatedAction;
        DutyFailedEvents.Changed -= this.DutyFailedAction;
        this.Common.Dispose();
        this.UI.Dispose();
    }

    public void Update(Configuration configuration)
    {
        this.UI.Update();
        this.Statistics.Update(configuration ?? new());
        this.CharacterUpdate();

        if (this.InDeepDungeon.IsActivated)
        {
            this.CheckForSolo();
            this.CheckForCharacterStats();
            this.CheckForBossKilled();
            this.CheckForMapReveal();
            this.CheckForTimeBonus();
            this.CheckForCairnOfPassageActivation();
            this.CheckForBossStatusTimer();
            this.CheckForNearbyEnemies();
            this.CheckForScoreWindowKills();
        }
        else
        {
            if (this.Common.IsInDeepDungeonRegion)
            {
                this.Common.CheckForSaveSlotDeletion();

                if (ServiceUtility.IsSolo)
                {
                    if (!this.InDutyQueue.IsActivated)
                    {
                        this.Common.CheckForSaveSlotSelection();
                    }
                }
                else
                {
                    this.Common.ResetSaveSlotSelection();
                }
            }
        }
    }

    private void CharacterUpdate() => this.Common.CharacterUpdate();

    private void CheckForSolo() => this.Common.CheckForSolo();

    private void CheckForCharacterStats()
    {
        if (this.IsCharacterBusy)
            return;

        this.Common.CheckForCharacterStats();
    }

    private void CheckForBossKilled()
    {
        if (this.IsCharacterBusy)
            return;

        this.Common.CheckForBossKilled(this.Text);
    }

    private void CheckForMapReveal()
    {
        if (this.IsCharacterBusy)
            return;

        this.Common.CheckForMapReveal();
    }

    private void CheckForTimeBonus()
    {
        if (this.IsCharacterBusy)
            return;

        this.Common.CheckForTimeBonus();
    }

    private void CheckForCairnOfPassageActivation()
    {
        if (this.IsCharacterBusy)
            return;

        this.Common.CheckForCairnOfPassageActivation(this.Text);
    }

    private void CheckForBossStatusTimer()
    {
        if (this.IsCharacterBusy)
            return;

        this.Common.CheckForBossStatusTimer(this.InCombat.IsActivated);
    }

    private void CheckForNearbyEnemies()
    {
        if (this.IsCharacterBusy)
            return;

        this.Common.CheckForNearbyEnemies(this.Text);
    }

    private void CheckForScoreWindowKills() => this.Common.CheckForScoreWindowKills();

    public void Login() => this.Common.ResetCharacterData();

    public void TerritoryChanged(ushort territoryType) => this.Common.DeepDungeonUpdate(this.Text, territoryType);

    public void ConditionChange(ConditionFlag flag, bool value)
    {
        switch (flag)
        {
            case ConditionFlag.BetweenAreas:
                this.BetweenAreas.Update(value);
                break;
            case ConditionFlag.InDutyQueue:
                this.InDutyQueue.Update(value);
                break;
            case ConditionFlag.InDeepDungeon:
                this.InDeepDungeon.Update(value);
                break;
            case ConditionFlag.Occupied33:
                this.Occupied33.Update(value);
                break;
            case ConditionFlag.InCombat:
                this.InCombat.Update(value);
                break;
            default:
                break;
        }
    }

    public void ChatMessage(string message)
    {
        if (this.InDeepDungeon.IsActivated)
        {
            this.EnchantmentMessage.Execute(message);
            this.TrapMessage.Execute(message);
        }
    }

    public void DutyStarted(ushort dutyId)
    {
        if (this.InDeepDungeon.IsActivated)
        {
            this.Common.DutyStarted();
        }
    }

    public void DutyCompleted()
    {
        if (this.InDeepDungeon.IsActivated)
        {
            this.Common.DutyCompleted();
        }
    }

    public void InventoryChangedRaw(IReadOnlyCollection<InventoryEventArgs> inventoryEventArgs)
    {
        if (!this.InDeepDungeon.IsActivated)
            return;

        if (!this.Common.IsLastFloor)
        {
            if (!this.Common.IsBronzeCofferOpened)
                return;

            this.Common.IsBronzeCofferOpened = false;

            foreach (InventoryEventArgs e in inventoryEventArgs ?? [])
            {
                if (e.Type is GameInventoryEvent.Added or GameInventoryEvent.Changed)
                {
                    var itemId = e.Item.ItemId;

                    var potsherdItemIds = new uint[] { 15422, 23164, 38941 };
                    if (potsherdItemIds.Contains(itemId))
                        this.Common.BronzeChestOpened(Coffer.Potsherd);
                    else
                        this.Common.BronzeChestOpened(Coffer.Medicine);
                }
            }
        }
    }

    private void DeepDungeonActivating()
    {
        this.Text.LoadEnchantments(DataText.GetLanguage());
        this.Common.EnteringDeepDungeon();
    }

    private void DeepDungeonDeactivating() => this.Common.ExitingDeepDungeon();

    private void CombatActivating() => this.Common.EnteringCombat();

    private void CombatDeactivating() => this.Common.ExitingCombat();

    private void EnchantmentMessageReceived(string message) => this.Common.EnchantmentMessageReceived(this.Text, message);

    private void TrapMessageReceived(string message) => this.Common.TrapMessageReceived(this.Text, message);

    private void PomanderChangedAction(object? sender, ItemChangedEventArgs<PomanderChangedType> args)
    {
        if (args.Type == PomanderChangedType.PomanderObtained)
        {
            this.Common.PomanderObtained(args.ItemId);
        }
        else if (args.Type == PomanderChangedType.PomanderUsed)
        {
            this.Common.PomanderUsed(args.ItemId);
        }
    }

    private void StoneChangedAction(object? sender, ItemChangedEventArgs<StoneChangedType> args)
    {
        if (args.Type == StoneChangedType.StoneObtained)
        {
            this.Common.StoneObtained(args.ItemId);
        }
        else if (args.Type == StoneChangedType.StoneUsed)
        {
            this.Common.StoneUsed(args.ItemId);
        }
    }

    private void FloorChangeAction(object? sender, NewFloorEventArgs args)
    {
        this.Common.StartNextFloor();
    }

    private void CharacterKilledAction(object? sender, CharacterKilledEventArgs args)
    {
        this.Common.CharacterKilled(this.Text, args.EntityId);
    }

    private void AetherpoolObtainedAction(object? sender, AetherpoolObtainedEventArgs args)
    {
        this.Common.AetherpoolObtained();
    }

    private void RegenPotionConsumedAction(object? sender, RegenPotionConsumedEventArgs args)
    {
        this.Common.RegenPotionConsumed();
    }

    private void TransferenceInitiatedAction(object? sender, TransferenceInitiatedEventArgs args)
    {
        this.Common.TransferenceInitiated();
    }

    private void DutyFailedAction(object? sender, DutyFailedEventArgs args)
    {
        this.Common.DutyFailed();
    }
}