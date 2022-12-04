﻿namespace DeepDungeonTracker
{
    public static class ScoreCreator
    {
        public static Score? Create(SaveSlot saveSlot, bool isDutyComplete)
        {
            if (saveSlot.DeepDungeon == DeepDungeon.ThePalaceOfTheDead)
                return new ScoreThePalaceOfTheDead(saveSlot, isDutyComplete);
            else if (saveSlot.DeepDungeon == DeepDungeon.HeavenOnHigh)
                return new ScoreHeavenOnHigh(saveSlot, isDutyComplete);
            else if (saveSlot.DeepDungeon == DeepDungeon.EurekaOrthos)
                return new ScoreEurekaOrthos(saveSlot, isDutyComplete);
            return null;
        }
    }
}