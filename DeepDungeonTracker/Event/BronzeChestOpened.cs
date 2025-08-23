using System;

namespace DeepDungeonTracker.Event
{
    public class BronzeChestOpenedEventArgs(Coffer coffer) : EventArgs
    {
        public Coffer Coffer { get; } = coffer;
    };

    public static class BronzeChestOpenedEvents
    {
        public static event EventHandler<BronzeChestOpenedEventArgs>? Changed;

        public static void Publish(Coffer coffer)
            => OnChanged(new BronzeChestOpenedEventArgs(coffer));

        private static void OnChanged(BronzeChestOpenedEventArgs e)
            => Changed?.Invoke(null, e);
    }
}