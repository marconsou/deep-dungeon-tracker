using System;
using System.Linq;

namespace DeepDungeonTracker
{
    public abstract class Score
    {
        private SaveSlot SaveSlot { get; }

        protected int Duty { get; }

        public int StartingFloorNumber { get; }

        public int CurrentFloorNumber { get; private set; }

        protected int DistanceFromStartingFloor { get; private set; }

        public int TotalReachedFloors { get; private set; }

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

        private static int DutyComplete => 101;

        public bool IsDutyComplete => this.Duty == DutyComplete;

        public int CurrentLevel => this.SaveSlot.CurrentLevel;

        public int AetherpoolArm => this.SaveSlot.AetherpoolArm;

        public int AetherpoolArmor => this.SaveSlot.AetherpoolArmor;

        public Score(SaveSlot saveSlot, bool isDutyComplete)
        {
            this.SaveSlot = saveSlot;
            var dutyFailed = Score.DutyComplete - ((saveSlot.KOs + 1) * 10);
            this.Duty = isDutyComplete ? Score.DutyComplete : dutyFailed;
            this.StartingFloorNumber = saveSlot.StartingFloorNumber();
            this.FloorCompletionUpdate(saveSlot.CurrentFloorNumber());
        }

        private void FloorCompletionUpdate(int currentFloorNumber)
        {
            this.CurrentFloorNumber = currentFloorNumber;
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

        private void CharacterScoreCalculation(int level) => this.CharacterScore = (this.AetherpoolArm + this.AetherpoolArmor) * 10 + level * 500;

        private void FloorScoreCalculation()
        {
            var total = 0;
            total += (430 - ((198 - this.AetherpoolArm - this.AetherpoolArmor) * 10)) * this.DistanceFromStartingFloor;
            total += (int)(this.CurrentFloorNumber - (this.StartingFloorNumber + Math.Truncate(this.DistanceFromStartingFloor / 10.0))) * 50 * 91;
            total += (int)Math.Truncate(this.DistanceFromStartingFloor / 10.0) * this.Duty * 300;
            total += this.FloorCompletionScoreCalculation();
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
            var normalFloors = floors.Where(x => x.Number >= 1 && x.Number <= this.LastNormalFloorNumber());
            var bonusFloors = floors.Where(x => x.Number >= this.LastNormalFloorNumber() + 1 && x.Number <= this.LastBonusFloorNumber());

            var killsBonusException = bonusFloors.Sum(x => x.Mimics + x.Mandragoras) + bonusFloors.Sum(x => x.NPCs) + bonusFloors.Where(x => x.IsLastFloor()).Sum(x => x.Kills);
            var kills = normalFloors.Sum(x => x.Kills) + killsBonusException;
            var killsBonus = bonusFloors.Sum(x => x.Kills) - killsBonusException;

            var floorBonus = (int)(Math.Truncate(this.TotalReachedFloors / 2.0) * this.KillScoreMultiplier()) + 100;

            this.KillScore = (floorBonus * kills) + ((floorBonus + this.Duty) * killsBonus);
        }

        public void TotalScoreCalculation(bool calculateScore, bool includeFloorCompletion)
        {
            if ((this.StartingFloorNumber != 1 && this.StartingFloorNumber != this.ShortcutStartingFloorNumber()) || !calculateScore)
            {
                this.TotalScore = 0;
                return;
            }

            var currentFloorNumber = this.CurrentFloorNumber;

            if (includeFloorCompletion)
                this.FloorCompletionUpdate(currentFloorNumber > this.LastNormalFloorNumber() ? this.LastBonusFloorNumber() : this.LastNormalFloorNumber());

            this.BaseScoreCalculation();
            this.CharacterScoreCalculation(!includeFloorCompletion ? this.CurrentLevel : this.Level());
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

            if (includeFloorCompletion)
                this.FloorCompletionUpdate(currentFloorNumber);
        }

        protected abstract int FloorCompletionScoreCalculation();

        protected abstract int Level();

        protected abstract int ShortcutStartingFloorNumber();

        protected abstract int LastNormalFloorNumber();

        protected abstract int LastBonusFloorNumber();

        protected abstract int KillScoreMultiplier();
    }
}