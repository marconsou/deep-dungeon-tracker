using System;

namespace DeepDungeonTracker.Event
{
    public class RegenPotionConsumedEventArgs() : EventArgs;

    public static class RegenPotionConsumedEvents
    {
        public static event EventHandler<RegenPotionConsumedEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new RegenPotionConsumedEventArgs());

        private static void OnChanged(RegenPotionConsumedEventArgs e)
            => Changed?.Invoke(null, e);
    }
}