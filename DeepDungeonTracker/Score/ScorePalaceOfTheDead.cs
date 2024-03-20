using System;

namespace DeepDungeonTracker;

public sealed class ScorePalaceOfTheDead(SaveSlot saveSlot, bool isDutyComplete) : Score(saveSlot, isDutyComplete)
{
    protected override int FloorCompletionScoreCalculation()
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

    protected override int Level() => 60;

    protected override int ShortcutStartingFloorNumber() => 51;

    protected override int LastNormalFloorNumber() => 100;

    protected override int LastBonusFloorNumber() => 200;

    protected override int KillScoreMultiplier() => 1;
}