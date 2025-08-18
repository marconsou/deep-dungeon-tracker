using System;

namespace DeepDungeonTracker.Event
{
    public class CharacterKilledEventArgs(uint entityId) : EventArgs
    {
        public uint EntityId { get; } = entityId;   
    }

    public static class CharacterKilledEvents
    {
        public static event EventHandler<CharacterKilledEventArgs>? Changed;

        public static void Publish(uint entityId)
            => OnChanged(new CharacterKilledEventArgs(entityId));

        private static void OnChanged(CharacterKilledEventArgs e)
            => Changed?.Invoke(null, e);
    }
}