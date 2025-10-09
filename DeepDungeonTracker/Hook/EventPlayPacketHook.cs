using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;
using System;

namespace DeepDungeonTracker.Hook
{
    public sealed class EventPlayPacketHook : IDisposable
    {
        private delegate void HandleEventPlayPacketDelegate(ulong param1, uint param2, ushort param3, ulong param4, ulong param5, byte param6);

        private readonly Hook<HandleEventPlayPacketDelegate>? _eventPlayPacketHookDelegate;

        public EventPlayPacketHook()
        {
            _eventPlayPacketHookDelegate =
                Service.GameInteropProvider.HookFromAddress<HandleEventPlayPacketDelegate>(
                    Service.SigScanner.ScanText(
                        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? B8 ?? ?? ?? ?? 49 8B F9"),
                    HandleEventPlayPacketDetour);
            _eventPlayPacketHookDelegate.Enable();
        }

        public void Dispose()
        {
            _eventPlayPacketHookDelegate?.Dispose();
        }

        private void HandleEventPlayPacketDetour(ulong param1, uint param2, ushort param3, ulong param4, ulong param5, byte param6)
        {
            _eventPlayPacketHookDelegate!.Original(param1, param2, param3, param4, param5, param6);

            if (!Service.Condition[ConditionFlag.InDeepDungeon])
                return;

            // Duty failed
            if (param3 == 5)
                DutyFailedEvents.Publish();
        }
    }
}