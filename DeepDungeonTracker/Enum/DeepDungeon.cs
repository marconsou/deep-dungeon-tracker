using System.ComponentModel;

namespace DeepDungeonTracker;

public enum DeepDungeon
{
    None,
    [Description("Palace of the Dead")]
    PalaceOfTheDead,
    [Description("Heaven-on-High")]
    HeavenOnHigh,
    [Description("Eureka Orthos")]
    EurekaOrthos
}