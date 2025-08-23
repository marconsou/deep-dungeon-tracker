using Dalamud.Hooking;
using DeepDungeonTracker.Event;
using System;

namespace DeepDungeonTracker.Hook
{
    public sealed class PacketEventPlayHook : IDisposable
    {
        private delegate void ProcessPacketEventPlayDelegate(ulong param1, uint param2, ushort param3, ulong param4, ulong param5, byte param6);

        private readonly Hook<ProcessPacketEventPlayDelegate>? _packetEventPlayHookDelegate;

        public PacketEventPlayHook()
        {
            _packetEventPlayHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketEventPlayDelegate>(
                    Service.SigScanner.ScanText(
                        "E8 ?? ?? ?? ?? B0 ?? 48 8B 5C 24 ?? 48 8B 74 24 ?? 48 83 C4 ?? 5F C3 80 7B"),
                    ProcessPacketEventPlayDetour);
            _packetEventPlayHookDelegate.Enable();
        }

        public void Dispose()
        {
            _packetEventPlayHookDelegate?.Dispose();
        }

        private void ProcessPacketEventPlayDetour(ulong param1, uint param2, ushort param3, ulong param4, ulong param5, byte param6)
        {
            _packetEventPlayHookDelegate!.Original(param1, param2, param3, param4, param5, param6);
            // Duty failed
            if (param3 == 5)
            {
                DutyFailedEvents.Publish();
            }
        }
    }
}