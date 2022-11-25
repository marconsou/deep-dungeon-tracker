using System;

namespace DeepDungeonTracker
{
    public class ConditionEvent
    {
        private bool Current { get; set; }

        private bool Previous { get; set; }

        public bool IsActivated => this.Current;

        private event Action? Activating;

        private event Action? Deactivating;

        public void AddActivating(Action? action) => this.Activating += action;

        public void RemoveActivating(Action? action) => this.Activating -= action;

        public void AddDeactivating(Action? action) => this.Deactivating += action;

        public void RemoveDeactivating(Action? action) => this.Deactivating -= action;

        public void Update(bool value)
        {
            this.Current = value;

            if (!this.Previous && this.Current)
                this.Activating?.Invoke();
            else if (this.Previous && !this.Current)
                this.Deactivating?.Invoke();

            this.Previous = this.Current;
        }
    }
}