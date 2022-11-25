namespace DeepDungeonTracker
{
    public sealed class ScoreDataHeavenOnHigh : ScoreData
    {
        public ScoreDataHeavenOnHigh(SaveSlot saveSlot, bool isDutyComplete) : base(saveSlot, isDutyComplete) { }

        public override int FloorScore()
        {
            var result = 0;
            return result;
        }

        public override bool IsValidStartingFloor() => this.StartingFloorNumber == 1 || this.StartingFloorNumber == 21;

        public override bool IsNormalFloor(Floor floor) => floor.Number >= 1 && floor.Number <= 30;

        public override bool IsBonusFloor(Floor floor) => floor.Number >= 31 && floor.Number <= 100;

        public override int KillScoreMultiplier() => 2;
    }
}