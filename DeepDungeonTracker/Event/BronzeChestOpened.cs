using System;

namespace DeepDungeonTracker.Event
{
    public class BronzeChestOpenedEventArgs() : EventArgs;

    public static class BronzeChestOpenedEvents
    {
        public static event EventHandler<BronzeChestOpenedEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new BronzeChestOpenedEventArgs());

        private static void OnChanged(BronzeChestOpenedEventArgs e)
            => Changed?.Invoke(null, e);
    }
}