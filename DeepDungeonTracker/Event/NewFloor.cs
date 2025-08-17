using System;

namespace DeepDungeonTracker.Event
{
    public class NewFloorEventArgs() : EventArgs;

    public static class NewFloorEvents
    {
        public static event EventHandler<NewFloorEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new NewFloorEventArgs());

        private static void OnChanged(NewFloorEventArgs e)
            => Changed?.Invoke(null, e);
    }
}