namespace DeepDungeonTracker
{
    public sealed class ScoreDataThePalaceOfTheDead : ScoreData
    {
        public ScoreDataThePalaceOfTheDead(SaveSlot saveSlot, bool isDutyComplete) : base(saveSlot, isDutyComplete) { }

        public override int FloorScore()
        {
            var result = 0;
            return result;
        }

        public override bool IsValidStartingFloor() => this.StartingFloorNumber == 1 || this.StartingFloorNumber == 51;

        public override bool IsNormalFloor(Floor floor) => floor.Number >= 1 && floor.Number <= 100;

        public override bool IsBonusFloor(Floor floor) => floor.Number >= 101 && floor.Number <= 200;

        public override int KillScoreMultiplier() => 1;
    }
}