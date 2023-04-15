using System;

namespace DeepDungeonTracker;

public static class TimeSpanExtension
{
    public static TimeSpan Round(this TimeSpan value)
    {
        var roundTo = TimeSpan.FromSeconds(1);
        return Math.Round(value / roundTo) * roundTo;
    }
}