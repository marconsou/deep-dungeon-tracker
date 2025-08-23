using Dalamud.Hooking;
using DeepDungeonTracker.Event;
using System;

namespace DeepDungeonTracker.Hook
{
    public sealed unsafe class PacketOpenTreasureHook : IDisposable
    {
        private delegate void ProcessPacketOpenTreasureDelegate(uint param1, byte* param2);

        private readonly Hook<ProcessPacketOpenTreasureDelegate>? _packetOpenTreasureHookDelegate;

        public PacketOpenTreasureHook()
        {
            _packetOpenTreasureHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketOpenTreasureDelegate>(
                    Service.SigScanner.ScanText(
                        "E8 ?? ?? ?? ?? B0 ?? 48 8B 5C 24 ?? 48 8B 74 24 ?? 48 83 C4 ?? 5F C3 8B 4F ?? E8"),
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
            BronzeChestManagerEvents.Publish();
        }
    }
}