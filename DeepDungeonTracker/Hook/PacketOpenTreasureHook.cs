using System;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;

namespace DeepDungeonTracker.Hook
{
    public unsafe class PacketOpenTreasureHook : IDisposable
    {
        private delegate void ProcessPacketOpenTreasureDelegate(uint param1, byte* param2);

        private readonly Hook<ProcessPacketOpenTreasureDelegate>? _PacketOpenTreasureHookDelegate;

        public PacketOpenTreasureHook()
        {
            _PacketOpenTreasureHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketOpenTreasureDelegate>(
                    Service.SigScanner.ScanText(
                        "E8 ?? ?? ?? ?? B0 ?? 48 8B 5C 24 ?? 48 8B 74 24 ?? 48 83 C4 ?? 5F C3 8B 4F ?? E8"),
                    ProcessPacketOpenTreasureDetour);
            _PacketOpenTreasureHookDelegate.Enable();
        }

        public void Dispose()
        {
            _PacketOpenTreasureHookDelegate?.Dispose();
        }

        private void ProcessPacketOpenTreasureDetour(uint param1, byte* param2)
        {
            _PacketOpenTreasureHookDelegate.Original(param1, param2);
            BronzeChestOpenedEvents.Publish();
        }
    }
}