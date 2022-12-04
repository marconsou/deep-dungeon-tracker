using System;
using System.Linq;

namespace DeepDungeonTracker
{
    public abstract class Score
    {
        public SaveSlot SaveSlot { get; }

        public int Duty { get; }

        public int StartingFloorNumber { get; }

        public int CurrentFloorNumber { get; }

        public int DistanceFromStartingFloor { get; }

        public int TotalReachedFloors { get; }

        public int BaseScore { get; private set; }

        public int CharacterScore { get; private set; }

        public int FloorScore { get; private set; }

        public int MapScore { get; private set; }

        public int CofferScore { get; private set; }

        public int NPCScore { get; private set; }

        public int MimicgoraScore { get; private set; }

        public int EnchantmentScore { get; private set; }

        public int TrapScore { get; private set; }

        public int TimeBonusScore { get; private set; }

        public int DeathScore { get; private set; }

        public int NonKillScore { get; private set; }

        public int KillScore { get; private set; }

        public int TotalScore { get; private set; }

        private int DutyFailed { get; }

        private static int DutyComplete => 101;

        public bool IsDutyComplete => this.Duty == DutyComplete;

        public Score(SaveSlot saveSlot, bool isDutyComplete)
        {
            this.SaveSlot = saveSlot;
            this.DutyFailed = Score.DutyComplete - ((saveSlot.KOs + 1) * 10);
            this.Duty = isDutyComplete ? Score.DutyComplete : this.DutyFailed;
            this.StartingFloorNumber = saveSlot.StartingFloorNumber();
            this.CurrentFloorNumber = saveSlot.CurrentFloorNumber();
            this.DistanceFromStartingFloor = this.CurrentFloorNumber - this.StartingFloorNumber;
            this.TotalReachedFloors = this.DistanceFromStartingFloor + 1;
        }

        private void BaseScoreCalculation()
        {
            var result = Math.Truncate(this.CurrentFloorNumber / 10.0) == (this.CurrentFloorNumber / 10.0) && this.IsDutyComplete ? this.Duty * 250 : 0;
            if (this.SaveSlot.Maps() >= 2 || (Math.Truncate(this.DistanceFromStartingFloor / 10.0) * this.Duty * 250 + result) / this.Duty * 250 != 0)
                this.BaseScore = 0;
            else
                this.BaseScore = -10;
        }

        private void CharacterScoreCalculation() => this.CharacterScore = (this.SaveSlot.AetherpoolArm + this.SaveSlot.AetherpoolArmor) * 10 + this.SaveSlot.CurrentLevel * 500;

        private void FloorScoreCalculation()
        {
            var total = 0;
            total += (430 - ((198 - this.SaveSlot.AetherpoolArm - this.SaveSlot.AetherpoolArmor) * 10)) * this.DistanceFromStartingFloor;
            total += (int)(this.CurrentFloorNumber - (this.StartingFloorNumber + Math.Truncate(this.DistanceFromStartingFloor / 10.0))) * 50 * 91;

            if (this.SaveSlot.CurrentLevel > 61)
            {
                if (this.CurrentFloorNumber == 100)
                    total += 50 * this.Duty;
            }
            else
            {
                if (this.CurrentFloorNumber == 200)
                    total += 50 * this.Duty;
            }

            total += (int)Math.Truncate(this.DistanceFromStartingFloor / 10.0) * this.Duty * 300;
            total += Math.Truncate(this.CurrentFloorNumber / 10.0) == (this.CurrentFloorNumber / 10.0) && this.IsDutyComplete && (this.SaveSlot.CurrentLevel > 61 ? this.CurrentFloorNumber != 100 : this.CurrentFloorNumber != 200) ? this.Duty * 300 : 0;

            if (this.SaveSlot.CurrentLevel > 61)
            {
                var temp1 = Math.Truncate(this.CurrentFloorNumber / 10.0) == (this.CurrentFloorNumber / 10.0) && this.IsDutyComplete ? this.Duty * 300 : 0;
                if (((Math.Truncate((this.DistanceFromStartingFloor) / 10.0) * this.Duty * 300 + temp1) / (this.Duty * 300)) >= 3 - Math.Truncate(this.StartingFloorNumber / 10.0))
                    total += 450 * this.Duty;

                var temp2 = Math.Truncate(this.CurrentFloorNumber / 10.0) == (this.CurrentFloorNumber / 10.0) && this.IsDutyComplete ? this.Duty * 300 : 0;
                if (((Math.Truncate((this.DistanceFromStartingFloor) / 10.0) * this.Duty * 300 + temp2) / this.Duty * 300) >= 1)
                    total += -50 * this.Duty;

                if (this.CurrentFloorNumber > 60 || (this.CurrentFloorNumber == 60 && this.IsDutyComplete))
                    total += 50 * this.Duty;

                if (this.CurrentFloorNumber > 70 || (this.CurrentFloorNumber == 70 && this.IsDutyComplete))
                    total += -50 * this.Duty;
            }
            else
            {
                if (this.StartingFloorNumber == 1)
                    total += -this.Duty * 50 * (int)Math.Min(Math.Truncate(this.CurrentFloorNumber / 10.0), 1.0);

                if (this.StartingFloorNumber == 1)
                {
                    if (this.CurrentFloorNumber > 50 || (this.CurrentFloorNumber == 50 && this.IsDutyComplete))
                        total += this.Duty * 450;
                }

                if (this.CurrentFloorNumber > 100 || (this.CurrentFloorNumber == 100 && this.IsDutyComplete))
                    total += this.Duty * 450;

                if (this.TotalReachedFloors == 50 && this.IsDutyComplete)
                    total += -2000;

                if (this.TotalReachedFloors == 200 && this.IsDutyComplete)
                    total += -9500 + 3200 * this.Duty;

                if (this.CurrentFloorNumber == 200 && this.TotalReachedFloors == 150 && this.IsDutyComplete)
                    total += -7000 + 3200 * this.Duty;
            }

            if (this.SaveSlot.CurrentLevel > 61)
            {
                if (this.TotalReachedFloors == 30 && this.IsDutyComplete)
                    total += -1000;
            }

            if (this.SaveSlot.CurrentLevel < 61)
            {
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
            }

            if (this.TotalReachedFloors == 100 && this.IsDutyComplete)
            {
                total += -4500;
                if (this.SaveSlot.CurrentLevel > 61)
                {
                    total += 3200 * this.Duty;
                }
            }
            this.FloorScore = total;
        }

        private void MapScoreCalculation()
        {
            var total = 0;
            if (this.BaseScore == 0)
            {
                var maps = this.SaveSlot.Maps();
                if (this.TotalReachedFloors > 10)
                    total += this.Duty * maps * 25;
                else
                {
                    if (this.IsDutyComplete)
                        total += this.Duty * maps * 25;
                    else
                        total += this.Duty * (maps - 2) * 25;
                }
            }
            this.MapScore = total;
        }

        private void CofferScoreCalculation() => this.CofferScore = this.BaseScore == 0 ? this.SaveSlot.Coffers() * this.Duty : 0;

        private void NPCScoreCalculation() => this.NPCScore = this.BaseScore == 0 ? this.SaveSlot.NPCs() * this.Duty * 20 : 0;

        private void MimicgoraScoreCalculation() => this.MimicgoraScore = this.BaseScore == 0 ? (this.SaveSlot.Mimics() + this.SaveSlot.Mandragoras()) * this.Duty * 5 : 0;

        private void EnchantmentScoreCalculation() => this.EnchantmentScore = this.BaseScore == 0 ? this.SaveSlot.Enchantments() * this.Duty * 5 : 0;

        private void TrapScoreCalculation() => this.TrapScore = this.BaseScore == 0 ? -this.SaveSlot.Traps() * this.Duty * 2 : 0;

        private void TimeBonusScoreCalculation() => this.TimeBonusScore = this.SaveSlot.TimeBonuses() * this.Duty * 150;

        private void DeathScoreCalculation() => this.DeathScore = this.BaseScore == 0 ? -this.SaveSlot.Deaths() * this.Duty * 50 : 0;

        private void NonKillScoreCalculation()
        {
            this.NonKillScore = (this.MapScore + this.CofferScore + this.NPCScore + this.MimicgoraScore + this.EnchantmentScore + this.TrapScore + this.TimeBonusScore + this.DeathScore) / this.Duty;

            if (this.NonKillScore > 0)
                this.NonKillScore = this.CharacterScore + this.FloorScore + this.MapScore + this.CofferScore + this.NPCScore + this.MimicgoraScore + this.EnchantmentScore + this.TrapScore + this.TimeBonusScore + this.DeathScore;
            else
                this.NonKillScore = this.CharacterScore + this.FloorScore + this.BaseScore;
        }

        private void KillScoreCalculation()
        {
            var floors = this.SaveSlot.FloorSets.SelectMany(x => x.Floors);
            var normalFloors = floors.Where(this.IsNormalFloor);
            var bonusFloors = floors.Where(this.IsBonusFloor);

            var killsBonusException = bonusFloors.Sum(x => x.Mimics + x.Mandragoras) + bonusFloors.Sum(x => x.NPCs) + bonusFloors.Where(x => x.IsLastFloor()).Sum(x => x.Kills);
            var kills = normalFloors.Sum(x => x.Kills) + killsBonusException;
            var killsBonus = bonusFloors.Sum(x => x.Kills) - killsBonusException;

            var floorBonus = (int)(Math.Truncate(this.TotalReachedFloors / 2.0) * this.KillScoreMultiplier()) + 100;

            this.KillScore = (floorBonus * kills) + ((floorBonus + this.Duty) * killsBonus);
        }

        public void TotalScoreCalculation(bool calculateScore)
        {
            if (!this.IsValidStartingFloor() || !calculateScore)
            {
                this.TotalScore = 0;
                return;
            }

            this.BaseScoreCalculation();
            this.CharacterScoreCalculation();
            this.FloorScoreCalculation();
            this.MapScoreCalculation();
            this.CofferScoreCalculation();
            this.NPCScoreCalculation();
            this.MimicgoraScoreCalculation();
            this.EnchantmentScoreCalculation();
            this.TrapScoreCalculation();
            this.TimeBonusScoreCalculation();
            this.DeathScoreCalculation();
            this.NonKillScoreCalculation();
            this.KillScoreCalculation();
            this.TotalScore = this.NonKillScore + this.KillScore;
        }

        protected abstract bool IsValidStartingFloor();

        protected abstract bool IsNormalFloor(Floor floor);

        protected abstract bool IsBonusFloor(Floor floor);

        protected abstract int KillScoreMultiplier();
    }
}