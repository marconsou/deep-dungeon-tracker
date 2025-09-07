using System;
using System.Threading.Tasks;

namespace DeepDungeonTracker.Event
{
    public static class BronzeChestManagerEvents
    {
        private static DateTime? lastPotsherdObtained;
        private static readonly TimeSpan timeout = TimeSpan.FromMilliseconds(500);

        static BronzeChestManagerEvents()
        {
            PotsherdObtainedEvents.Changed += PotsherdObtainedAction;
        }

        static void PotsherdObtainedAction(object? sender, PotsherdObtainedEventArgs e)
        {
            lastPotsherdObtained = DateTime.Now;
        }

        public static async void Publish()
        {
            await Task.Delay(timeout).ConfigureAwait(false);

            if (lastPotsherdObtained.HasValue && Math.Abs((lastPotsherdObtained.Value - DateTime.Now).TotalMilliseconds) < timeout.TotalMilliseconds)
            {
                BronzeChestOpenedEvents.Publish(Coffer.Potsherd);
            }
            else
            {
                BronzeChestOpenedEvents.Publish(Coffer.Medicine);
            }
        }
    }
}