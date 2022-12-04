namespace DeepDungeonTracker
{
    public sealed class ScoreEurekaOrthos : Score
    {
        public ScoreEurekaOrthos(SaveSlot saveSlot, bool isDutyComplete) : base(saveSlot, isDutyComplete) { }

        protected override bool IsValidStartingFloor() => this.StartingFloorNumber == 1 || this.StartingFloorNumber == 21;

        protected override bool IsNormalFloor(Floor floor) => floor.Number >= 1 && floor.Number <= 30;

        protected override bool IsBonusFloor(Floor floor) => floor.Number >= 31 && floor.Number <= 100;

        protected override int KillScoreMultiplier() => 2;
    }
}