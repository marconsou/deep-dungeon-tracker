using System;

namespace DeepDungeonTracker
{
    public sealed class ScoreEurekaOrthos : Score
    {
        public ScoreEurekaOrthos(SaveSlot saveSlot, bool isDutyComplete) : base(saveSlot, isDutyComplete) { }

        protected override int FloorCompletionScoreCalculation()
        {
            var total = 0;
            total += Math.Truncate(this.CurrentFloorNumber / 10.0) == (this.CurrentFloorNumber / 10.0) && this.IsDutyComplete && this.CurrentFloorNumber != 100 ? this.Duty * 300 : 0;

            var result = Math.Truncate(this.CurrentFloorNumber / 10.0) == (this.CurrentFloorNumber / 10.0) && this.IsDutyComplete ? this.Duty * 300 : 0;

            if (((Math.Truncate((this.DistanceFromStartingFloor) / 10.0) * this.Duty * 300 + result) / (this.Duty * 300)) >= 3 - Math.Truncate(this.StartingFloorNumber / 10.0))
                total += 450 * this.Duty;

            if (((Math.Truncate((this.DistanceFromStartingFloor) / 10.0) * this.Duty * 300 + result) / this.Duty * 300) >= 1)
                total += -50 * this.Duty;

            if (this.CurrentFloorNumber > 60 || (this.CurrentFloorNumber == 60 && this.IsDutyComplete))
                total += 50 * this.Duty;

            if (this.CurrentFloorNumber > 70 || (this.CurrentFloorNumber == 70 && this.IsDutyComplete))
                total += -50 * this.Duty;

            if (this.CurrentFloorNumber == 100)
                total += 50 * this.Duty;

            if (this.TotalReachedFloors == 30 && this.IsDutyComplete)
                total += -1000;

            if (this.TotalReachedFloors == 100 && this.IsDutyComplete)
            {
                total += -4500;
                total += 3200 * this.Duty;
            }
            return total;
        }

        protected override int Level() => 90;

        protected override int ShortcutStartingFloorNumber() => 21;

        protected override int LastNormalFloorNumber() => 30;

        protected override int LastBonusFloorNumber() => 100;

        protected override int KillScoreMultiplier() => 2;
    }
}