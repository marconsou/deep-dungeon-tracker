using System;

namespace DeepDungeonTracker.Event
{
    public class AetherpoolObtainedEventArgs() : EventArgs;

    public static class AetherpoolObtainedEvents
    {
        public static event EventHandler<AetherpoolObtainedEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new AetherpoolObtainedEventArgs());

        private static void OnChanged(AetherpoolObtainedEventArgs e)
            => Changed?.Invoke(null, e);
    }
}