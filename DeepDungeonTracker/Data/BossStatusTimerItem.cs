using System;
using System.Text.Json.Serialization;

namespace DeepDungeonTracker;

public class BossStatusTimerItem
{
    [JsonInclude]
    public BossStatusTimer BossStatusTimer { get; private set; }

    [JsonInclude]
    public DateTime Start { get; private set; }

    [JsonInclude]
    public DateTime End { get; private set; }

    [JsonInclude]
    public byte Stacks { get; private set; }

    public bool HasStarted() => this.Start != default;

    public bool HasEnded() => this.End != default;

    public bool IsDataValid() => this.HasStarted() && this.HasEnded();

    public TimeSpan Duration() => this.IsDataValid() ? this.End - this.Start : default;

    public void StacksUpdate(byte stacks) => this.Stacks = stacks;

    public BossStatusTimerItem(BossStatusTimer bossStatusTimer)
    {
        this.BossStatusTimer = bossStatusTimer;
        this.Start = DateTime.Now;
    }

    public void TimerEnd()
    {
        if (!this.HasEnded())
            this.End = DateTime.Now;
    }
}