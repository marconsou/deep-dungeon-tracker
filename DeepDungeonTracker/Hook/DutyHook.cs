using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;
using System;
namespace DeepDungeonTracker.Hook
{
    // Took from DutyState.cs of Dalamud project
    public sealed unsafe class DutyHook : IDisposable
    {
        private delegate byte SetupContentDirectNetworkMessageDelegate(IntPtr a1, IntPtr a2, ushort* a3);

        private readonly Hook<SetupContentDirectNetworkMessageDelegate>? _dutyHookDelegate;

        public DutyHook()
        {
            _dutyHookDelegate = Service.GameInteropProvider.HookFromAddress<SetupContentDirectNetworkMessageDelegate>(
                Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 33 FF 48 8B D9 41 0F B7 08"),
                DetourDutyHook);
            _dutyHookDelegate.Enable();
        }

        public void Dispose()
        {
            _dutyHookDelegate?.Dispose();
        }

        private byte DetourDutyHook(IntPtr a1, IntPtr a2, ushort* a3)
        {
            var category = *a3;
            var type = *(uint*)(a3 + 4);

            if (category == 0x6D)
            {
                switch (type)
                {
                    // New floor
                    case 0x4000_000E:
                        if (Service.Condition[ConditionFlag.InDeepDungeon])
                            NewFloorEvents.Publish();
                        break;
                }
            }

            return this._dutyHookDelegate!.Original(a1, a2, a3);
        }
    }
}