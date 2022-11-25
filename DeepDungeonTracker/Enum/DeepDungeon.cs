using System.ComponentModel;

namespace DeepDungeonTracker
{
    public enum DeepDungeon
    {
        None,
        [Description("The Palace of the Dead")]
        ThePalaceOfTheDead,
        [Description("Heaven-on-High")]
        HeavenOnHigh,
        [Description("Eureka Orthos")]
        EurekaOrthos
    }
}