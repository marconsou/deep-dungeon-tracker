
using System;

namespace DeepDungeonTracker.Event
{
    public enum PomanderChangedType
    {
        PomanderObtained,
        PomanderUsed,
    }
    
    public class PomanderChangedEventArgs : EventArgs
    {
        public PomanderChangedEventArgs(PomanderChangedType type, byte itemId)
        {
            this.Type = type;
            this.ItemId = itemId;
        }

        public PomanderChangedType Type { get; }
        public byte ItemId { get; }
    }
    
    public static class PomanderChangedEvents
    {
        public static event EventHandler<PomanderChangedEventArgs>? PomanderChanged;

        public static void Publish(PomanderChangedType type, byte itemId)
            => OnChanged(new PomanderChangedEventArgs(type, itemId));
        
        private static void OnChanged(PomanderChangedEventArgs e)
            => PomanderChanged?.Invoke(null, e);
    }
}