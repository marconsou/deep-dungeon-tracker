using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using DeepDungeonTracker.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

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

    private Event<string> AetherpoolMessage { get; } = new();

    private Event<string> TransferenceInitiatedMessage { get; } = new();

    private Event<string> DutyFailedMessage { get; } = new();

    private Event<(IntPtr, uint)> ActorControl { get; } = new();

    private Event<(IntPtr, uint)> ActorControlSelf { get; } = new();

    private Event<(IntPtr, uint)> EventStart { get; } = new();

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
        this.AetherpoolMessage.Add(this.AetherpoolMessageReceived);
        this.TransferenceInitiatedMessage.Add(this.TransferenceInitiatedMessageReceived);
        this.DutyFailedMessage.Add(this.DutyFailedMessageReceived);
        this.ActorControl.Add(this.ActorControlAction);
        this.ActorControlSelf.Add(this.ActorControlSelfAction);
        this.UnknownBronzeCofferItemInfo.Add(this.UnknownBronzeCofferItemInfoAction);
        this.UnknownBronzeCofferOpen.Add(this.UnknownBronzeCofferOpenAction);
        ItemChangedEvents<PomanderChangedType>.Changed += this.PomanderChangedAction;
        ItemChangedEvents<StoneChangedType>.Changed += this.StoneChangedAction;
        NewFloorEvents.Changed += this.FloorChangeAction;

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
        this.AetherpoolMessage.Remove(this.AetherpoolMessageReceived);
        this.TransferenceInitiatedMessage.Remove(this.TransferenceInitiatedMessageReceived);
        this.ActorControl.Remove(this.ActorControlAction);
        this.ActorControlSelf.Remove(this.ActorControlSelfAction);
        this.UnknownBronzeCofferItemInfo.Remove(this.UnknownBronzeCofferItemInfoAction);
        this.UnknownBronzeCofferOpen.Remove(this.UnknownBronzeCofferOpenAction);
        ItemChangedEvents<PomanderChangedType>.Changed -= this.PomanderChangedAction;
        ItemChangedEvents<StoneChangedType>.Changed -= this.StoneChangedAction;
        NewFloorEvents.Changed -= this.FloorChangeAction;
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
            this.CheckForPomandersChanged();
            this.CheckForStonesChanged();
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

    private void CheckForPomandersChanged() => this.Common.CheckForPomandersChanged();

    private void CheckForStonesChanged() => this.Common.CheckForStonesChanged();

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

    public void InventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        foreach (var e in events)
        {
            Service.PluginLog.Info(
                $"type: {e.Type}, address: {e.Item.Address}, itemId: {e.Item.ItemId}, quantity: {e.Item.Quantity}, containerType: {e.Item.ContainerType}");
        }

        ImmutableList<uint> regenPotionsItemIds = ImmutableList.Create<uint>(20309, 23163, 38944);
        if (this.InDeepDungeon.IsActivated)
        {
            foreach (var e in events)
            {
                if (regenPotionsItemIds.Contains(e.Item.ItemId) && (e.Type is GameInventoryEvent.Changed or GameInventoryEvent.Removed))
                {
                    this.Common.RegenPotionConsumed();
                }
            }
        }

    }

    public void DutyStarted(ushort dutyId)
    {
        if (this.InDeepDungeon.IsActivated)
        {
            Service.PluginLog.Info($"DeepDungeonTracker: Duty Started. Content ID: {dutyId}");
            this.Common.DutyStarted();
        }
    }
    
    public void DutyRecommenced()
    {
        if (this.InDeepDungeon.IsActivated)
        {
            Service.PluginLog.Info($"DeepDungeonTracker: Duty recommence");
            this.Common.StartNextFloor();
        }
    }

    public void DutyCompleted()
    {
        if (this.InDeepDungeon.IsActivated)
        {
            Service.PluginLog.Info("DeepDungeonTracker: Duty completed.");
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
            if (opCode == opCodes!.ActorControl)
                this.ActorControl.Execute((dataPtr, targetActorId));
            else if (opCode == opCodes.ActorControlSelf)
                this.ActorControlSelf.Execute((dataPtr, targetActorId));
            else if (opCode == opCodes.EventStart)
                this.EventStart.Execute((dataPtr, targetActorId));
            else if (opCode == opCodes.UnknownBronzeCofferItemInfo)
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

    private void AetherpoolMessageReceived(string message) => this.Common.AetherpoolMessageReceived(this.Text, message);

    private void TransferenceInitiatedMessageReceived(string message) => this.Common.TransferenceInitiatedMessageReceived(this.Text, message);

    private void DutyFailedMessageReceived(string message) => this.Common.DutyFailedMessageReceived(this.Text, message);

    private void ActorControlAction((IntPtr, uint) data)
    {
        var defeat = 6;
        if (NetworkData.ExtractNumber(data.Item1, 0, 1) == defeat)
        {
            var id = data.Item2;
            var character = Service.ObjectTable.SearchById(id) as ICharacter;
            var name = character?.Name.TextValue ?? string.Empty;
            if ((character?.ObjectKind == ObjectKind.BattleNpc) && (character.StatusFlags.HasFlag(StatusFlags.Hostile) || this.Text.IsMandragora(name).Item1))
            {
                if (!this.Common.IsBossFloor)
                    this.Common.CheckForEnemyKilled(this.Text, name, id);
            }
            else if (character?.ObjectKind == ObjectKind.Player)
                this.Common.CheckForPlayerKilled(character);
        }
    }

    private void ActorControlSelfAction((IntPtr, uint) data)
    {
        var directorUpdate = 109;
        var dutyCommenced = 1;
        var dutyRecommence = 6;
        var deepDungeonIdFirst = 60001;
        var deepDungeonIdLast = 60000 + (Enum.GetNames(typeof(DeepDungeon)).Length * 10);
        var dataPtr = data.Item1;
        if (NetworkData.ExtractNumber(dataPtr, 0, 1) == directorUpdate)
        {
            var type = NetworkData.ExtractNumber(dataPtr, 8, 1);
            if (type == dutyCommenced)
            {
                var contentId = NetworkData.ExtractNumber(dataPtr, 4, 2);
                if (contentId >= deepDungeonIdFirst && contentId <= deepDungeonIdLast)
                    this.Common.StartFirstFloor(contentId);
            }
            else if (type == dutyRecommence)
                this.Common.StartNextFloor();
        }
    }

    private void UnknownBronzeCofferItemInfoAction((IntPtr, uint) data) => this.Common.BronzeCofferUpdate(this.Text, NetworkData.ExtractNumber(data.Item1, 8, 2));

    private void UnknownBronzeCofferOpenAction((IntPtr, uint) data) => this.Common.BronzeCofferOpened();

    private void PomanderChangedAction(object? sender, ItemChangedEventArgs<PomanderChangedType> args)
    {
        Service.PluginLog.Info("DeepDungeonTracker: Pomander changed: " + args.ItemId);
        if (args.Type == PomanderChangedType.PomanderObtained)
        {
            Service.PluginLog.Info("DeepDungeonTracker: Pomander obtained: " + args.ItemId);
            this.Common.PomanderObtained(args.ItemId);
        }
        else if (args.Type == PomanderChangedType.PomanderUsed)
        {
            Service.PluginLog.Info("DeepDungeonTracker: Pomander used: " + args.ItemId);
            this.Common.PomanderUsed(args.ItemId);
        }
    }

    private void StoneChangedAction(object? sender, ItemChangedEventArgs<StoneChangedType> args)
    {
        Service.PluginLog.Info("DeepDungeonTracker: Stone changed: " + args.ItemId);
        if (args.Type == StoneChangedType.StoneObtained)
        {
            Service.PluginLog.Info("DeepDungeonTracker: Stone obtained: " + args.ItemId);
            this.Common.StoneObtained(args.ItemId);
        }
        else if (args.Type == StoneChangedType.StoneUsed)
        {
            Service.PluginLog.Info("DeepDungeonTracker: Stone used: " + args.ItemId);
            this.Common.StoneUsed(args.ItemId);
        }
    }

    private void FloorChangeAction(object? sender, NewFloorEventArgs args)
    {
        this.Common.StartNextFloor();   
    }
}