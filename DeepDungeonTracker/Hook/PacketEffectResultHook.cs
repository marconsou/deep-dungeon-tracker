using System;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;

namespace DeepDungeonTracker.Hook
{
    public sealed unsafe class PacketEffectResultHook : IDisposable
    {
        private delegate void ProcessPacketEffectResultDelegate(uint param1, byte* param2, byte param3);

        private readonly Hook<ProcessPacketEffectResultDelegate>? _packetEffectResultHookDelegate;

        public PacketEffectResultHook()
        {
            _packetEffectResultHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketEffectResultDelegate>(
                    Service.SigScanner.ScanText(
                        "48 8B C4 44 88 40 ?? 89 48"),
                    ProcessPacketEffectResultDetour);
            _packetEffectResultHookDelegate.Enable();
        }

        public void Dispose()
        {
            _packetEffectResultHookDelegate?.Dispose();
        }

        private void ProcessPacketEffectResultDetour(uint param1, byte* param2, byte param3)
        {
            _packetEffectResultHookDelegate!.Original(param1, param2, param3);
            var id = *(ushort*)(param2 + 30);
            if (id == 648)
                RegenPotionConsumedEvents.Publish();

        }
    }
}