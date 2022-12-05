namespace DeepDungeonTracker
{
    public sealed class ScoreEurekaOrthos : Score
    {
        public ScoreEurekaOrthos(SaveSlot saveSlot, bool isDutyComplete) : base(saveSlot, isDutyComplete) { }

        protected override int FloorCompletionScoreCalculation()
        {
            var total = 0;
            return total;
        }

        protected override int Level() => 90;

        protected override int ShortcutStartingFloorNumber() => 21;

        protected override int LastNormalFloorNumber() => 30;

        protected override int LastBonusFloorNumber() => 100;

        protected override int KillScoreMultiplier() => 2;
    }
}