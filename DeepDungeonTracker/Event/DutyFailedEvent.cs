using System;

namespace DeepDungeonTracker.Event
{
    public class DutyFailedEventArgs() : EventArgs;

    public static class DutyFailedEvents
    {
        public static event EventHandler<DutyFailedEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new DutyFailedEventArgs());

        private static void OnChanged(DutyFailedEventArgs e)
            => Changed?.Invoke(null, e);
    }
}