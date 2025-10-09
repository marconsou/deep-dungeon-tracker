using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using System;

namespace DeepDungeonTracker.Hook
{
    public sealed unsafe class OpenTreasurePacketHook : IDisposable
    {
        private delegate void HandleOpenTreasurePacketDelegate(uint param1, byte* param2);

        private readonly Hook<HandleOpenTreasurePacketDelegate>? _openTreasurePacketHookDelegate;

        private DataCommon DataCommon { get; set; }

        public OpenTreasurePacketHook(DataCommon dataCommon)
        {
            this.DataCommon = dataCommon;
            _openTreasurePacketHookDelegate =
                Service.GameInteropProvider.HookFromAddress<HandleOpenTreasurePacketDelegate>(
                    Service.SigScanner.ScanText(
                        "40 53 48 83 EC ?? 48 8B DA 48 8D 0D ?? ?? ?? ?? 8B 52 ?? E8 ?? ?? ?? ?? 48 85 C0 74 ?? F3 0F 10 5B"),
                    HandleOpenTreasurePacketDetour);
            _openTreasurePacketHookDelegate.Enable();
        }

        public void Dispose()
        {
            _openTreasurePacketHookDelegate?.Dispose();
        }

        private void HandleOpenTreasurePacketDetour(uint param1, byte* param2)
        {
            _openTreasurePacketHookDelegate!.Original(param1, param2);

            if (!Service.Condition[ConditionFlag.InDeepDungeon])
                return;

            this.DataCommon.IsBronzeCofferOpened = true;
        }
    }
}