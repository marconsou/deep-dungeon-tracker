using System;

namespace DeepDungeonTracker.Event
{
    public class TransferenceInitiatedEventArgs() : EventArgs;

    public static class TransferenceInitiatedEvents
    {
        public static event EventHandler<TransferenceInitiatedEventArgs>? Changed;

        public static void Publish()
            => OnChanged(new TransferenceInitiatedEventArgs());

        private static void OnChanged(TransferenceInitiatedEventArgs e)
            => Changed?.Invoke(null, e);
    }
}