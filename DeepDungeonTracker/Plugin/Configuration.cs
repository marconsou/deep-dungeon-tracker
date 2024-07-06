using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Linq;
using System.Numerics;

namespace DeepDungeonTracker;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public MainTab Main { get; set; } = new();

    public TrackerTab Tracker { get; set; } = new();

    public FloorSetTimeTab FloorSetTime { get; set; } = new();

    public ScoreTab Score { get; set; } = new();

    public StatisticsTab Statistics { get; set; } = new();

    public BossStatusTimerTab BossStatusTimer { get; set; } = new();

    public OpCodeValues OpCodes { get; set; } = new();

    public class MainTab
    {
        public bool SolidBackground { get; set; }

        public float Scale { get; set; } = 1.0f;
    }

    public class TrackerTab
    {
        public class Field
        {
            public bool Show { get; set; } = true;

            public int Index { get; set; }
        }

        public bool Lock { get; set; }

        public bool SolidBackground { get; set; }

        public bool Show { get; set; }

        public bool ShowInBetweenFloors { get; set; }

        public bool ShowFloorEffectPomanders { get; set; } = true;

        public FontType FontType { get; set; }

        public float Scale { get; set; } = 1.0f;

        public bool IsFloorNumberVisible { get; set; } = true;

        public Vector4 FloorNumberColor { get; set; } = Color.White;

        public bool IsSetNumberVisible { get; set; } = true;

        public Vector4 SetNumberColor { get; set; } = Color.White;

        public bool IsTotalNumberVisible { get; set; } = true;

        public Vector4 TotalNumberColor { get; set; } = Color.White;

#pragma warning disable CA1819
        public Field[] Fields { get; set; } = Enumerable.Repeat(new Field(), 14).Select((x, index) => new Field() { Show = (index != 3), Index = index }).ToArray();
#pragma warning restore CA1819
    }

    public class FloorSetTimeTab
    {
        public bool Lock { get; set; }

        public bool SolidBackground { get; set; }

        public bool Show { get; set; }

        public bool ShowInBetweenFloors { get; set; }

        public bool ShowTitle { get; set; } = true;

        public bool ShowFloorTime { get; set; } = true;

        public float Scale { get; set; } = 1.0f;

        public Vector4 PreviousFloorTimeColor { get; set; } = Color.White;

        public Vector4 CurrentFloorTimeColor { get; set; } = Color.Gold;

        public Vector4 AverageTimeColor { get; set; } = Color.White;

        public Vector4 RespawnTimeColor { get; set; } = Color.Red;
    }

    public class ScoreTab
    {
        public bool Lock { get; set; }

        public bool SolidBackground { get; set; }

        public bool Show { get; set; }

        public bool ShowInBetweenFloors { get; set; }

        public bool ShowTitle { get; set; } = true;

        public FontType FontType { get; set; }

        public ScoreCalculationType ScoreCalculationType { get; set; }

        public float Scale { get; set; } = 1.0f;

        public bool IsFlyTextScoreVisible { get; set; } = true;

        public Vector4 FlyTextScoreColor { get; set; } = new(0.0f, 0.666f, 0.0f, 1.0f);

        public Vector4 TotalScoreColor { get; set; } = Color.White;
    }

    public class StatisticsTab
    {
        public bool SolidBackground { get; set; }

        public float Scale { get; set; } = 1.0f;

        public Vector4 FloorTimeColor { get; set; } = Color.Gold;

        public Vector4 ScoreColor { get; set; } = Color.Green;

        public Vector4 SummarySelectionColor { get; set; } = Color.Cyan;

        public bool ShowThreeRoomsFloor { get; set; } = true;

        public bool ShowFourRoomsFloor { get; set; } = true;

        public bool ShowFiveRoomsFloor { get; set; } = true;

        public bool ShowSixRoomsFloor { get; set; } = true;

        public bool ShowSevenRoomsFloor { get; set; } = true;

        public bool ShowEightRoomsFloor { get; set; } = true;
    }

    public class BossStatusTimerTab
    {
        public bool SolidBackground { get; set; }

        public float Scale { get; set; } = 1.0f;

        public bool IsStartTimeVisible { get; set; } = true;

        public Vector4 StartTimeColor { get; set; } = Color.Green;

        public bool IsEndTimeVisible { get; set; } = true;

        public Vector4 EndTimeColor { get; set; } = Color.Red;

        public Vector4 TotalTimeColor { get; set; } = Color.Gold;
    }

    public class OpCodeValues
    {
        public ushort ActorControl { get; set; }

        public ushort ActorControlSelf { get; set; }

        public ushort Effect { get; set; }

        public ushort EventStart { get; set; }

        public ushort SystemLogMessage { get; set; }

        public ushort UnknownBronzeCofferItemInfo { get; set; }

        public ushort UnknownBronzeCofferOpen { get; set; }
    }

    public Configuration() => this.Reset();

    public void Initialize(IDalamudPluginInterface pluginInterface) => this.PluginInterface = pluginInterface;

    public void Save() => this.PluginInterface!.SavePluginConfig(this);

    public void Reset()
    {
        this.Main = new();
        this.Tracker = new();
        this.FloorSetTime = new();
        this.Score = new();
        this.Statistics = new();
        this.BossStatusTimer = new();
    }
}