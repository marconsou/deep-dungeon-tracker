using System;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;

namespace DeepDungeonTracker.Hook
{
    public unsafe class PacketEffectResultlHook : IDisposable
    {
        private delegate void ProcessPacketEffectResultlDelegate(uint param1, IntPtr param2, byte param3);

        private readonly Hook<ProcessPacketEffectResultlDelegate>? _PacketEffectResultlHookDelegate;

        public PacketEffectResultlHook()
        {
            _PacketEffectResultlHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketEffectResultlDelegate>(
                    Service.SigScanner.ScanText(
                        "48 8B C4 44 88 40 ?? 89 48"),
                    ProcessPacketEffectResultlDetour);
            _PacketEffectResultlHookDelegate.Enable();
        }

        public void Dispose()
        {
            _PacketEffectResultlHookDelegate?.Dispose();
        }

        private void ProcessPacketEffectResultlDetour(uint param1, IntPtr param2, byte param3)
        {
            _PacketEffectResultlHookDelegate.Original(param1, param2, param3);
            var id = NetworkData.ExtractNumber(param2, 30, 2);
            if (id == 648)
                RegenPotionConsumedEvents.Publish();
            
        }
    }
}