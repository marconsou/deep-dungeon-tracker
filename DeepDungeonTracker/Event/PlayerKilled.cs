using System;

namespace DeepDungeonTracker.Event
{
    public class PlayerKilledEventArgs() : EventArgs;

    public static class PlayerKilledEvents
    {
        public static event EventHandler<PlayerKilledEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new PlayerKilledEventArgs());

        private static void OnChanged(PlayerKilledEventArgs e)
            => Changed?.Invoke(null, e);
    }
}