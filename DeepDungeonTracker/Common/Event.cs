using System;

namespace DeepDungeonTracker
{
    public class Event<T>
    {
        private event Action<T>? Action;

        public void Add(Action<T>? action) => this.Action += action;

        public void Remove(Action<T>? action) => this.Action -= action;

        public void Execute(T data) => this.Action?.Invoke(data);
    }
}