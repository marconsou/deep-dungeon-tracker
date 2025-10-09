using Dalamud.Game.ClientState.Conditions;
using Dalamud.Hooking;
using DeepDungeonTracker.Event;
using System;

namespace DeepDungeonTracker.Hook
{
    public sealed unsafe class EffectResultPacketHook : IDisposable
    {
        private delegate void HandleEffectResultPacketDelegate(uint param1, byte* param2, byte param3);

        private readonly Hook<HandleEffectResultPacketDelegate>? _effectResultPacketHookDelegate;

        public EffectResultPacketHook()
        {
            _effectResultPacketHookDelegate =
                Service.GameInteropProvider.HookFromAddress<HandleEffectResultPacketDelegate>(
                    Service.SigScanner.ScanText(
                        "48 8B C4 44 88 40 ?? 89 48"),
                    HandleEffectResultPacketDetour);
            _effectResultPacketHookDelegate.Enable();
        }

        public void Dispose()
        {
            _effectResultPacketHookDelegate?.Dispose();
        }

        private void HandleEffectResultPacketDetour(uint param1, byte* param2, byte param3)
        {
            _effectResultPacketHookDelegate!.Original(param1, param2, param3);

            if (!Service.Condition[ConditionFlag.InDeepDungeon])
                return;

            var id = *(ushort*)(param2 + 30);
            if (id == 648)
                RegenPotionConsumedEvents.Publish();
        }
    }
}