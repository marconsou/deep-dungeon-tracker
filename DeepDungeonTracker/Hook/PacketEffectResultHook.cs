using System;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;

namespace DeepDungeonTracker.Hook
{
    public unsafe class PacketEffectResultHook : IDisposable
    {
        private delegate void ProcessPacketEffectResultDelegate(uint param1, byte* param2, byte param3);

        private readonly Hook<ProcessPacketEffectResultDelegate>? _PacketEffectResultHookDelegate;

        public PacketEffectResultHook()
        {
            _PacketEffectResultHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketEffectResultDelegate>(
                    Service.SigScanner.ScanText(
                        "48 8B C4 44 88 40 ?? 89 48"),
                    ProcessPacketEffectResultDetour);
            _PacketEffectResultHookDelegate.Enable();
        }

        public void Dispose()
        {
            _PacketEffectResultHookDelegate?.Dispose();
        }

        private void ProcessPacketEffectResultDetour(uint param1, byte* param2, byte param3)
        {
            _PacketEffectResultHookDelegate.Original(param1, param2, param3);
            var id = *(ushort*)(param2 + 30);
            if (id == 648)
                RegenPotionConsumedEvents.Publish();
            
        }
    }
}