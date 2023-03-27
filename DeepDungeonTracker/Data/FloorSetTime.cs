using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepDungeonTracker;

public class FloorSetTime
{
    private static TimeSpan InstanceTime => new(0, 59, 59);

    private DateTime? StartTime { get; set; }

    private DateTime? PauseTime { get; set; }

    public ICollection<TimeSpan> PreviousFloorsTime { get; } = new List<TimeSpan>();

    public TimeSpan TotalTime => new(Math.Min(((this.PauseTime ?? DateTime.Now) - (this.StartTime ?? DateTime.Now)).Ticks, FloorSetTime.InstanceTime.Ticks));

    public TimeSpan CurrentFloorTime => new((this.TotalTime - new TimeSpan(this.PreviousFloorsTime.Sum(x => x.Ticks))).Ticks / 10000000 * 10000000);

    public TimeSpan Average => new(this.TotalTime.Ticks / (this.PreviousFloorsTime.Count + 1));

    public void Start()
    {
        this.StartTime = DateTime.Now;
        this.PauseTime = null;
        this.PreviousFloorsTime.Clear();
    }

    public void Pause()
    {
        if (!this.PauseTime.HasValue)
            this.PauseTime = DateTime.Now;
    }

    public TimeSpan AddFloor()
    {
        var time = new TimeSpan(this.CurrentFloorTime.Ticks);
        this.PreviousFloorsTime.Add(time);
        return time;
    }
}