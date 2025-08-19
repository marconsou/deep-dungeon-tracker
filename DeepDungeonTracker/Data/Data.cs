using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dalamud.Game;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using DeepDungeonTracker.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace DeepDungeonTracker;

public sealed unsafe class Data : IDisposable
{
    public DataCommon Common { get; } = new();

    private DataOpCodes OpCodes { get; } = new();

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

    private Event<string> TransferenceInitiatedMessage { get; } = new();

    private Event<(IntPtr, uint)> UnknownBronzeCofferItemInfo { get; } = new();

    private Event<(IntPtr, uint)> UnknownBronzeCofferOpen { get; } = new();

    private bool IsCharacterBusy => this.BetweenAreas.IsActivated || this.Occupied33.IsActivated || this.Common.IsTransferenceInitiated;

    public bool IsInsideDeepDungeon => this.InDeepDungeon.IsActivated;

    public bool IsInDeepDungeonSubArea =>
        this.Text.IsPalaceOfTheDeadRegion(Service.ClientState.TerritoryType, true) ||
        this.Text.IsHeavenOnHighRegion(Service.ClientState.TerritoryType, true) ||
        this.Text.IsEurekaOrthosRegion(Service.ClientState.TerritoryType, true);

    public Data(IUiBuilder uiBuilder, Configuration configuration)
    {
        this.UI = new(uiBuilder);
        this.OpCodes.Load(configuration).ConfigureAwait(true);

        this.InDeepDungeon.AddActivating(this.DeepDungeonActivating);
        this.InDeepDungeon.AddDeactivating(this.DeepDungeonDeactivating);
        this.InCombat.AddActivating(this.CombatActivating);
        this.InCombat.AddDeactivating(this.CombatDeactivating);
        this.EnchantmentMessage.Add(this.EnchantmentMessageReceived);
        this.TrapMessage.Add(this.TrapMessageReceived);
        this.TransferenceInitiatedMessage.Add(this.TransferenceInitiatedMessageReceived);
        this.UnknownBronzeCofferItemInfo.Add(this.UnknownBronzeCofferItemInfoAction);
        this.UnknownBronzeCofferOpen.Add(this.UnknownBronzeCofferOpenAction);
        ItemChangedEvents<PomanderChangedType>.Changed += this.PomanderChangedAction;
        ItemChangedEvents<StoneChangedType>.Changed += this.StoneChangedAction;
        AetherpoolObtainedEvents.Changed += this.AetherpoolObtainedAction;
        NewFloorEvents.Changed += this.FloorChangeAction;
        CharacterKilledEvents.Changed += this.CharacterKilledAction;
        RegenPotionConsumedEvents.Changed += this.RegenPotionConsumedAction;

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
        this.TransferenceInitiatedMessage.Remove(this.TransferenceInitiatedMessageReceived);
        this.UnknownBronzeCofferItemInfo.Remove(this.UnknownBronzeCofferItemInfoAction);
        this.UnknownBronzeCofferOpen.Remove(this.UnknownBronzeCofferOpenAction);
        ItemChangedEvents<PomanderChangedType>.Changed -= this.PomanderChangedAction;
        ItemChangedEvents<StoneChangedType>.Changed -= this.StoneChangedAction;
        AetherpoolObtainedEvents.Changed -= this.AetherpoolObtainedAction;
        NewFloorEvents.Changed -= this.FloorChangeAction;
        CharacterKilledEvents.Changed -= this.CharacterKilledAction;
        RegenPotionConsumedEvents.Changed -= this.RegenPotionConsumedAction;
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
            this.TransferenceInitiatedMessage.Execute(message);
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

    public void NetworkMessage(IntPtr dataPtr, ushort opCode, uint targetActorId, Configuration configuration)
    {
        if (this.InDeepDungeon.IsActivated)
        {
            if (this.Common.IsSoloSaveSlot)
            {
                this.OpCodes.FindUnknownBronzeCofferItemInfoOpCode(dataPtr, opCode, targetActorId, configuration!);
                this.OpCodes.FindUnknownBronzeCofferOpenOpCode(dataPtr, opCode, targetActorId, configuration!);
            }

            var opCodes = configuration?.OpCodes;
            if (opCode == opCodes!.UnknownBronzeCofferItemInfo)
                this.UnknownBronzeCofferItemInfo.Execute((dataPtr, targetActorId));
            else if (opCode == opCodes.UnknownBronzeCofferOpen)
                this.UnknownBronzeCofferOpen.Execute((dataPtr, targetActorId));
        }
    }

    private void DeepDungeonActivating() => this.Common.EnteringDeepDungeon();

    private void DeepDungeonDeactivating() => this.Common.ExitingDeepDungeon();

    private void CombatActivating() => this.Common.EnteringCombat();

    private void CombatDeactivating() => this.Common.ExitingCombat();

    private void EnchantmentMessageReceived(string message) => this.Common.EnchantmentMessageReceived(this.Text, message);

    private void TrapMessageReceived(string message) => this.Common.TrapMessageReceived(this.Text, message);

    private void TransferenceInitiatedMessageReceived(string message) => this.Common.TransferenceInitiatedMessageReceived(this.Text, message);

    private void UnknownBronzeCofferItemInfoAction((IntPtr, uint) data) => this.Common.BronzeCofferUpdate(this.Text, NetworkData.ExtractNumber(data.Item1, 8, 2));

    private void UnknownBronzeCofferOpenAction((IntPtr, uint) data) => this.Common.BronzeCofferOpened();

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
        this.Common.CharacterKilledAction(this.Text, args.EntityId);
    }
    
    private void AetherpoolObtainedAction(object? sender, AetherpoolObtainedEventArgs args)
    {
        this.Common.AetherpoolObtained();
    }
    
    private void RegenPotionConsumedAction(object? sender, RegenPotionConsumedEventArgs args)
    {
        this.Common.RegenPotionConsumed();
    }
}