namespace DeepDungeonTracker
{
    public sealed class ScoreThePalaceOfTheDead : Score
    {
        public ScoreThePalaceOfTheDead(SaveSlot saveSlot, bool isDutyComplete) : base(saveSlot, isDutyComplete) { }

        protected override bool IsValidStartingFloor() => this.StartingFloorNumber == 1 || this.StartingFloorNumber == 51;

        protected override bool IsNormalFloor(Floor floor) => floor.Number >= 1 && floor.Number <= 100;

        protected override bool IsBonusFloor(Floor floor) => floor.Number >= 101 && floor.Number <= 200;

        protected override int KillScoreMultiplier() => 1;
    }
}