using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Linq;

namespace DeepDungeonTracker
{
    public sealed class Data : IDisposable
    {
        public DataCommon Common { get; } = new();

        private DataOpCodes OpCodes { get; } = new();

        public DataUI UI { get; } = new();

        private DataText Text { get; } = new();

        public DataStatistics Statistics { get; } = new();

        private ConditionEvent BetweenAreas { get; } = new();

        private ConditionEvent BoundToDuty97 { get; } = new();

        private ConditionEvent InDeepDungeon { get; } = new();

        private ConditionEvent Occupied33 { get; } = new();

        private Event<string> EnchantmentMessage { get; } = new();

        private Event<string> TrapMessage { get; } = new();

        private Event<(IntPtr, uint)> ActorControl { get; } = new();

        private Event<(IntPtr, uint)> ActorControlSelf { get; } = new();

        private Event<(IntPtr, uint)> Effect { get; } = new();

        private Event<(IntPtr, uint)> EventStart { get; } = new();

        private Event<(IntPtr, uint)> SystemLogMessage { get; } = new();

        private Event<(IntPtr, uint)> UnknownBronzeCofferItemInfo { get; } = new();

        private Event<(IntPtr, uint)> UnknownBronzeCofferOpen { get; } = new();

        private bool IsCharacterBusy => this.BetweenAreas.IsActivated || this.Occupied33.IsActivated || this.Common.IsTransferenceInitiated;

        public bool IsInsideDeepDungeon => this.InDeepDungeon.IsActivated;

        public Data(Configuration configuration)
        {
            this.OpCodes.Load(configuration);

            this.InDeepDungeon.AddActivating(this.DeepDungeonActivating);
            this.InDeepDungeon.AddDeactivating(this.DeepDungeonDeactivating);
            this.EnchantmentMessage.Add(this.EnchantmentMessageReceived);
            this.TrapMessage.Add(this.TrapMessageReceived);
            this.ActorControl.Add(this.ActorControlAction);
            this.ActorControlSelf.Add(this.ActorControlSelfAction);
            this.Effect.Add(this.EffectAction);
            this.EventStart.Add(this.EventStartAction);
            this.SystemLogMessage.Add(this.SystemLogMessageAction);
            this.UnknownBronzeCofferItemInfo.Add(this.UnknownBronzeCofferItemInfoAction);
            this.UnknownBronzeCofferOpen.Add(this.UnknownBronzeCofferOpenAction);

            if (Service.ClientState.IsLoggedIn)
            {
                this.ConditionChange(ConditionFlag.BetweenAreas, Service.Condition[ConditionFlag.BetweenAreas]);
                this.ConditionChange(ConditionFlag.BoundToDuty97, Service.Condition[ConditionFlag.BoundToDuty97]);
                this.ConditionChange(ConditionFlag.InDeepDungeon, Service.Condition[ConditionFlag.InDeepDungeon]);
                this.ConditionChange(ConditionFlag.Occupied33, Service.Condition[ConditionFlag.Occupied33]);
                this.TerritoryChanged(Service.ClientState.TerritoryType);
                this.Login();
            }
        }

        public void Dispose()
        {
            this.InDeepDungeon.RemoveActivating(this.DeepDungeonActivating);
            this.InDeepDungeon.RemoveDeactivating(this.DeepDungeonDeactivating);
            this.EnchantmentMessage.Remove(this.EnchantmentMessageReceived);
            this.TrapMessage.Remove(this.TrapMessageReceived);
            this.ActorControl.Remove(this.ActorControlAction);
            this.ActorControlSelf.Remove(this.ActorControlSelfAction);
            this.Effect.Remove(this.EffectAction);
            this.EventStart.Remove(this.EventStartAction);
            this.SystemLogMessage.Remove(this.SystemLogMessageAction);
            this.UnknownBronzeCofferItemInfo.Remove(this.UnknownBronzeCofferItemInfoAction);
            this.UnknownBronzeCofferOpen.Remove(this.UnknownBronzeCofferOpenAction);
            this.UI.Dispose();
        }

        public void Update(Configuration configuration)
        {
            this.UI.Update(configuration.General.ShowAccurateTargetHPPercentage);
            this.CharacterUpdate();

            if (this.InDeepDungeon.IsActivated)
            {
                this.CheckForSolo();
                this.CheckForCharacterStats();
                this.CheckForMapReveal();
                this.CheckForTimeBonus();
                this.CheckForCairnOfPassageActivation();
            }
            else
            {
                if (this.Common.IsInDeepDungeonRegion)
                {
                    this.Common.CheckForSaveSlotDeletion();

                    if (ServiceUtility.IsSolo)
                    {
                        if (!this.BoundToDuty97.IsActivated)
                            this.Common.CheckForSaveSlotSelection();
                    }
                    else
                        this.Common.ResetSaveSlotSelection();
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

        private void CheckForMapReveal()
        {
            if (this.IsCharacterBusy || this.Common.IsLastFloor)
                return;

            this.Common.CheckForMapReveal();
        }

        private void CheckForTimeBonus()
        {
            if (this.IsCharacterBusy)
                return;

            this.Common.CheckForTimeBonus(this.Text);
        }

        private void CheckForCairnOfPassageActivation()
        {
            if (this.IsCharacterBusy)
                return;

            this.Common.CheckForCairnOfPassageActivation(this.Text);
        }

        public void Login() => this.Common.ResetCharacterData();

        public void TerritoryChanged(ushort territoryType) => this.Common.DeepDungeonUpdate(this.Text, territoryType);

        public void ConditionChange(ConditionFlag flag, bool value)
        {
            switch (flag)
            {
                case ConditionFlag.BetweenAreas:
                    this.BetweenAreas.Update(value);
                    break;
                case ConditionFlag.BoundToDuty97:
                    this.BoundToDuty97.Update(value);
                    break;
                case ConditionFlag.InDeepDungeon:
                    this.InDeepDungeon.Update(value);
                    break;
                case ConditionFlag.Occupied33:
                    this.Occupied33.Update(value);
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
                else if (opCode == opCodes.Effect)
                    this.Effect.Execute((dataPtr, targetActorId));
                else if (opCode == opCodes.EventStart)
                    this.EventStart.Execute((dataPtr, targetActorId));
                else if (opCode == opCodes.SystemLogMessage)
                    this.SystemLogMessage.Execute((dataPtr, targetActorId));
                else if (opCode == opCodes.UnknownBronzeCofferItemInfo)
                    this.UnknownBronzeCofferItemInfo.Execute((dataPtr, targetActorId));
                else if (opCode == opCodes.UnknownBronzeCofferOpen)
                    this.UnknownBronzeCofferOpen.Execute((dataPtr, targetActorId));
            }
        }

        private void DeepDungeonActivating() => this.Common.EnteringDeepDungeon();

        private void DeepDungeonDeactivating() => this.Common.ExitingDeepDungeon();

        private void EnchantmentMessageReceived(string message) => this.Common.EnchantmentMessageReceived(this.Text, message);

        private void TrapMessageReceived(string message) => this.Common.TrapMessageReceived(this.Text, message);

        private void ActorControlAction((IntPtr, uint) data)
        {
            var defeat = 6;
            if (NetworkData.ExtractNumber(data.Item1, 0, 1) == defeat)
            {
                var character = Service.ObjectTable.SearchById(data.Item2) as Character;
                var name = character?.Name.TextValue ?? string.Empty;
                if ((character?.ObjectKind == ObjectKind.BattleNpc) && (character.StatusFlags.HasFlag(StatusFlags.Hostile) || this.Text.IsMandragora(name).Item1))
                {
                    if (!this.Common.IsLastFloor)
                        this.Common.CheckForEnemyKilled(this.Text, name);
                    else
                        this.Common.CheckForBossKilled(this.Text, name);
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
            var deepDungeonIdLast = 60030;
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

        private void EffectAction((IntPtr, uint) data)
        {
            var dataPtr = data.Item1;
            var id = NetworkData.ExtractNumber(dataPtr, 8, 2);
            var regenPotionIds = new int[] { 20309, 23163 };
            if (regenPotionIds.Contains(id))
                this.Common.RegenPotionConsumed();
        }

        private void EventStartAction((IntPtr, uint) data) => this.Common.DutyFailed(NetworkData.ExtractNumber(data.Item1, 8, 2), NetworkData.ExtractNumber(data.Item1, 16, 1));

        private void SystemLogMessageAction((IntPtr, uint) data)
        {
            var dataPtr = data.Item1;
            var logId = NetworkData.ExtractNumber(dataPtr, 4, 4);
            var goldCofferPomander = new[] { 7220, 7221 };
            var silverCofferPomander = new[] { 9206, 9207 };
            var silverCofferAetherpool = new[] { 7250, 7251, 7252, 7253 };
            var usePomander = 7254;
            var useMagicite = 9209;
            var transferenceInitiated = 7248;
            var discoverItem = new[] { 7279, 7280 };

            if (goldCofferPomander.Contains(logId))
                this.Common.GoldCofferPomander(NetworkData.ExtractNumber(dataPtr, 12, 1));
            else if (silverCofferPomander.Contains(logId))
                this.Common.SilverCofferPomander(NetworkData.ExtractNumber(dataPtr, 12, 1));
            else if (silverCofferAetherpool.Contains(logId))
                this.Common.SilverCofferAetherpool();
            else if (logId == usePomander)
                this.Common.PomanderUsed(NetworkData.ExtractNumber(dataPtr, 16, 1));
            else if (logId == useMagicite)
                this.Common.MagiciteUsed(NetworkData.ExtractNumber(dataPtr, 16, 1));
            else if (logId == transferenceInitiated)
                this.Common.TransferenceInitiated();
            else if (discoverItem.Contains(logId))
                this.Common.DutyCompleted();
        }

        private void UnknownBronzeCofferItemInfoAction((IntPtr, uint) data) => this.Common.BronzeCofferUpdate(this.Text, NetworkData.ExtractNumber(data.Item1, 8, 2));

        private void UnknownBronzeCofferOpenAction((IntPtr, uint) data) => this.Common.BronzeCofferOpened();
    }
}