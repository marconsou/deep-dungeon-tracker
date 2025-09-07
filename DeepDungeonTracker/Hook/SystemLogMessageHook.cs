using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;
using System;
using System.Linq;

namespace DeepDungeonTracker.Hook
{
    public sealed unsafe class SystemLogMessageHook : IDisposable
    {
        private delegate void ProcessSystemLogMessageDelegate(uint param1, uint type, uint* param3, byte param4);

        private readonly Hook<ProcessSystemLogMessageDelegate>? _systemLogMessageHookDelegate;

        private static uint[] pomanderObtained = [7220, 7221];

        private static readonly uint[] AetherpoolObtained = [7250, 7251, 7252, 7253];
        private static readonly uint[] MagiciteObtained = [9206, 9207];
        private static readonly uint[] DemicloneObtained = [10285, 10286];
        private const uint PomanderUsed = 7254;
        private const uint MagiciteUsed = 9209;
        private const uint DemicloneUsed = 10288;
        private const uint TransferenceInitiated = 7248;

        public SystemLogMessageHook()
        {
            _systemLogMessageHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessSystemLogMessageDelegate>(
                    Service.SigScanner.ScanText(
                        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 41 0F B6 D9 49 8B F8 8B F2"),
                    ProcessSystemLogMessageDetour);
            _systemLogMessageHookDelegate.Enable();
        }

        public void Dispose()
        {
            _systemLogMessageHookDelegate?.Dispose();
        }

        private void ProcessSystemLogMessageDetour(uint param1, uint type, uint* param3, byte param4)
        {
            _systemLogMessageHookDelegate!.Original(param1, type, param3, param4);

            if (!Service.Condition[ConditionFlag.InDeepDungeon])
                return;

            var itemUsedId = (int)*(param3 + 1);
            var itemObtainedId = (int)*(param3);
            if (pomanderObtained.Contains(type))
                ItemChangedEvents<PomanderChangedType>.Publish(PomanderChangedType.PomanderObtained, itemObtainedId);
            else if (AetherpoolObtained.Contains(type))
                AetherpoolObtainedEvents.Publish();
            else if (MagiciteObtained.Contains(type) || DemicloneObtained.Contains(type))
                ItemChangedEvents<StoneChangedType>.Publish(StoneChangedType.StoneObtained, itemObtainedId);
            else if (type == PomanderUsed)
                ItemChangedEvents<PomanderChangedType>.Publish(PomanderChangedType.PomanderUsed, itemUsedId);
            else if (type is MagiciteUsed or DemicloneUsed)
                ItemChangedEvents<StoneChangedType>.Publish(StoneChangedType.StoneUsed, itemUsedId);
            else if (type == TransferenceInitiated)
                TransferenceInitiatedEvents.Publish();
        }
    }
}