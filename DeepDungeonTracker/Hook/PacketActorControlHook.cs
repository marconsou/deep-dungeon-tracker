using System;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;

namespace DeepDungeonTracker.Hook
{
    // Took from https://github.com/wolfcomp/AllaganKillFeed/blob/master/PacketCapture.cs
    public class PacketActorControlHook : IDisposable
    {
        private delegate void ProcessPacketActorControlDelegate(uint entityId, uint type, uint param1, uint param2,
            uint param3, uint param4, uint param5, uint param6, ulong param7, byte isReplay);

        private readonly Hook<ProcessPacketActorControlDelegate>? _PacketActorControlHookDelegate;

        public PacketActorControlHook()
        {
            _PacketActorControlHookDelegate =
                Service.GameInteropProvider.HookFromAddress<ProcessPacketActorControlDelegate>(
                    Service.SigScanner.ScanText(
                        "E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64"),
                    ProcessPacketActorControlDetour);
            _PacketActorControlHookDelegate.Enable();
        }

        public void Dispose()
        {
            _PacketActorControlHookDelegate?.Dispose();
        }

        private void ProcessPacketActorControlDetour(uint entityId, uint type, uint param1, uint param2, uint param3,
            uint param4, uint param5, uint param6, ulong param7, byte isReplay)
        {
            _PacketActorControlHookDelegate.Original(entityId, type, param1, param2, param3, param4, param5, param6,
                param7, isReplay);
            if (isReplay != 0) return; // Ignore replays
            switch (type)
            {
                case 0x6: // Death
                    CharacterKilledEvents.Publish(entityId);
                    break;
            }
        }
    }
}