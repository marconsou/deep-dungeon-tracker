using System;

namespace DeepDungeonTracker
{
    public sealed class ScoreThePalaceOfTheDead : Score
    {
        public ScoreThePalaceOfTheDead(SaveSlot saveSlot, bool isDutyComplete) : base(saveSlot, isDutyComplete) { }

        protected override int FloorScoreCalculation(bool includeFloorCompletion)
        {
            var total = 0;
            total += Math.Truncate(this.CurrentFloorNumber / 10.0) == (this.CurrentFloorNumber / 10.0) && this.IsDutyComplete && this.CurrentFloorNumber != 200 ? this.Duty * 300 : 0;

            if (this.StartingFloorNumber == 1)
            {
                total += -this.Duty * 50 * (int)Math.Min(Math.Truncate(this.CurrentFloorNumber / 10.0), 1.0);
                if (this.CurrentFloorNumber > 50 || (this.CurrentFloorNumber == 50 && this.IsDutyComplete))
                    total += this.Duty * 450;
            }

            if (this.CurrentFloorNumber > 100 || (this.CurrentFloorNumber == 100 && this.IsDutyComplete))
                total += this.Duty * 450;

            if (this.TotalReachedFloors == 50 && this.IsDutyComplete)
                total += -2000;

            if (this.TotalReachedFloors == 200 && this.IsDutyComplete)
                total += -9500 + 3200 * this.Duty;

            if (this.TotalReachedFloors == 150 && this.CurrentFloorNumber == 200 && this.IsDutyComplete)
                total += -7000 + 3200 * this.Duty;

            if (this.CurrentFloorNumber == 200)
                total += 50 * this.Duty;

            if (this.CurrentFloorNumber > 60 || (this.IsDutyComplete && this.CurrentFloorNumber == 60))
            {
                if (this.StartingFloorNumber == 51)
                {
                    total += -50 * this.Duty;
                }
                else
                {
                    if (this.CurrentFloorNumber < 100)
                        total += -50 * this.Duty;
                }
            }

            if (this.TotalReachedFloors == 100 && this.IsDutyComplete)
                total += -4500;

            return total;
        }

        protected override bool IsValidStartingFloor() => this.StartingFloorNumber == 1 || this.StartingFloorNumber == 51;

        protected override bool IsNormalFloor(Floor floor) => floor.Number >= 1 && floor.Number <= 100;

        protected override bool IsBonusFloor(Floor floor) => floor.Number >= 101 && floor.Number <= 200;

        protected override int KillScoreMultiplier() => 1;
    }
}