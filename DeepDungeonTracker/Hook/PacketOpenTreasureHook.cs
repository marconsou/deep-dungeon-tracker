using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using System;

namespace DeepDungeonTracker.Hook
{
    public sealed unsafe class PacketOpenTreasureHook : IDisposable
    {
        private delegate void ProcessPacketOpenTreasureDelegate(uint param1, byte* param2);

        private readonly Hook<ProcessPacketOpenTreasureDelegate>? _packetOpenTreasureHookDelegate;

        private DataCommon DataCommon { get; set; }

        public PacketOpenTreasureHook(DataCommon dataCommon)
        {
            this.DataCommon = dataCommon;
            _packetOpenTreasureHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketOpenTreasureDelegate>(
                    Service.SigScanner.ScanText(
                        "E8 ?? ?? ?? ?? B0 ?? 48 8B 5C 24 ?? 48 8B 74 24 ?? 48 83 C4 ?? 5F C3 B8"),
                    ProcessPacketOpenTreasureDetour);
            _packetOpenTreasureHookDelegate.Enable();
        }

        public void Dispose()
        {
            _packetOpenTreasureHookDelegate?.Dispose();
        }

        private void ProcessPacketOpenTreasureDetour(uint param1, byte* param2)
        {
            _packetOpenTreasureHookDelegate!.Original(param1, param2);

            if (!Service.Condition[ConditionFlag.InDeepDungeon])
                return;

            this.DataCommon.IsBronzeCofferOpened = true;
        }
    }
}