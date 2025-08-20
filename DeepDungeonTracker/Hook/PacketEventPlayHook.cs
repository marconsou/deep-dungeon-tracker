using System;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;

namespace DeepDungeonTracker.Hook
{
    public unsafe class PacketEventPlayHook : IDisposable
    {
        private delegate void ProcessPacketEventPlayDelegate(ulong param1, uint param2, ushort param3, ulong param4, ulong param5, byte param6);

        private readonly Hook<ProcessPacketEventPlayDelegate>? _PacketEventPlayHookDelegate;

        public PacketEventPlayHook()
        {
            _PacketEventPlayHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketEventPlayDelegate>(
                    Service.SigScanner.ScanText(
                        "E8 ?? ?? ?? ?? B0 ?? 48 8B 5C 24 ?? 48 8B 74 24 ?? 48 83 C4 ?? 5F C3 80 7B"),
                    ProcessPacketEventPlayDetour);
            _PacketEventPlayHookDelegate.Enable();
        }

        public void Dispose()
        {
            _PacketEventPlayHookDelegate?.Dispose();
        }

        private void ProcessPacketEventPlayDetour(ulong param1, uint param2, ushort param3, ulong param4, ulong param5, byte param6)
        {
            _PacketEventPlayHookDelegate.Original(param1, param2, param3, param4, param5, param6);
            // Duty failed
            if (param3 == 5)
            {
                DutyFailedEvents.Publish();
            }
        }
    }
}