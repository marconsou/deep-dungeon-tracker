using System.ComponentModel;

namespace DeepDungeonTracker
{
    public enum ScoreCalculationType
    {
        [Description("Current Floor")]
        CurrentFloor,
        [Description("Score Window Floor")]
        ScoreWindowFloor,
        [Description("Last Floor")]
        LastFloor
    }
}