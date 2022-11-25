namespace DeepDungeonTracker
{
    public abstract class ScoreData
    {
        public SaveSlot SaveSlot { get; }

        public int Duty { get; }

        public int StartingFloorNumber { get; }

        public int CurrentFloorNumber { get; }

        public int DistanceFromStartingFloor { get; }

        public int TotalReachedFloors { get; }

        private readonly int DutyComplete = 101;

        private readonly int DutyFailed;

        public bool IsDutyComplete => this.Duty == this.DutyComplete;

        public ScoreData(SaveSlot saveSlot, bool isDutyComplete)
        {
            this.SaveSlot = saveSlot;
            this.DutyFailed = this.DutyComplete - ((saveSlot.KOs + 1) * 10);
            this.Duty = isDutyComplete ? this.DutyComplete : this.DutyFailed;
            this.StartingFloorNumber = saveSlot.StartingFloorNumber();
            this.CurrentFloorNumber = saveSlot.CurrentFloorNumber();
            this.DistanceFromStartingFloor = this.CurrentFloorNumber - this.StartingFloorNumber;
            this.TotalReachedFloors = this.DistanceFromStartingFloor + 1;
        }

        public abstract int FloorScore();

        public abstract bool IsValidStartingFloor();

        public abstract bool IsNormalFloor(Floor floor);

        public abstract bool IsBonusFloor(Floor floor);

        public abstract int KillScoreMultiplier();
    }
}