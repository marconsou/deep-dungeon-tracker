using System;

namespace DeepDungeonTracker.Event
{
    public class PotsherdObtainedEventArgs() : EventArgs;

    public static class PotsherdObtainedEvents
    {
        public static event EventHandler<PotsherdObtainedEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new PotsherdObtainedEventArgs());

        private static void OnChanged(PotsherdObtainedEventArgs e)
            => Changed?.Invoke(null, e);
    }
}