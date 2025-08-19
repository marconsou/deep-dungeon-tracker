using System;
using System.Linq;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;

namespace DeepDungeonTracker.Hook
{
    public unsafe class SystemLogMessageHook : IDisposable
    {
        private delegate void ProcessSystemLogMessageDelegate(uint param1, uint type, uint* param3, byte param4);

        private readonly Hook<ProcessSystemLogMessageDelegate>? _SystemLogMessageHookDelegate;
        
        public static uint[] pomanderObtained = [7220, 7221];
        
        public static readonly uint[] aetherpoolObtained = [7250, 7251, 7252, 7253];
        public static readonly uint[] magiciteObtained = [9206, 9207];
        public static readonly uint[] demicloneObtained = [10285, 10286];
        public static readonly uint pomanderUsed = 7254;
        public static readonly uint magiciteUsed = 9209;
        public static readonly uint demicloneUsed = 10288;

        public SystemLogMessageHook()
        {
            _SystemLogMessageHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessSystemLogMessageDelegate>(
                    Service.SigScanner.ScanText(
                        "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 41 0F B6 D9 49 8B F8 8B F2"),
                    ProcessSystemLogMessageDetour);
            _SystemLogMessageHookDelegate.Enable();
        }

        public void Dispose()
        {
            _SystemLogMessageHookDelegate?.Dispose();
        }

        private void ProcessSystemLogMessageDetour(uint param1, uint type, uint* param3, byte param4)
        {
            _SystemLogMessageHookDelegate.Original(param1, type, param3, param4);
            var itemUsedId = (int)*(param3 + 1);
            var itemObtainedId = (int)*(param3);
            if (pomanderObtained.Contains(type))
                ItemChangedEvents<PomanderChangedType>.Publish(PomanderChangedType.PomanderObtained, itemObtainedId);
            else if (aetherpoolObtained.Contains(type))
                AetherpoolObtainedEvents.Publish();
            else if (magiciteObtained.Contains(type) || demicloneObtained.Contains(type))
                ItemChangedEvents<StoneChangedType>.Publish(StoneChangedType.StoneObtained, itemObtainedId);
            else if (type == pomanderUsed)
                ItemChangedEvents<PomanderChangedType>.Publish(PomanderChangedType.PomanderUsed, itemUsedId);
            else if (type == magiciteUsed || type == demicloneUsed)
                ItemChangedEvents<StoneChangedType>.Publish(StoneChangedType.StoneUsed, itemObtainedId);
        }
    }
}