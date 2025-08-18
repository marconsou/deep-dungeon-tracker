using System;

namespace DeepDungeonTracker.Event
{
    public class EnemyKilledEventArgs(string name) : EventArgs
    {
        public string Name { get; } = name;   
    }

    public static class EnemyKilledEvents
    {
        public static event EventHandler<EnemyKilledEventArgs>? Changed;

        public static void Publish(string name)
            => OnChanged(new EnemyKilledEventArgs(name));

        private static void OnChanged(EnemyKilledEventArgs e)
            => Changed?.Invoke(null, e);
    }
}