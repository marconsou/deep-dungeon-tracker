using System;
using System.Linq;

namespace DeepDungeonTracker
{
    public static class Score
    {
        private static bool ShowLogOnce { get; set; }

        private static int BaseScore(ScoreData scoreData)
        {
            var result = Math.Truncate(scoreData.CurrentFloorNumber / 10.0) == (scoreData.CurrentFloorNumber / 10.0) && scoreData.Duty == 101 ? scoreData.Duty * 250 : 0;
            if (scoreData.SaveSlot.Maps() >= 2 || (Math.Truncate((scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber) / 10.0) * scoreData.Duty * 250 + result) / scoreData.Duty * 250 != 0)
                return 0;
            else
                return -10;
        }

        private static int CharacterScore(ScoreData scoreData) => (scoreData.SaveSlot.AetherpoolArm + scoreData.SaveSlot.AetherpoolArmor) * 10 + scoreData.SaveSlot.CurrentLevel * 500;

        private static int FloorScore(ScoreData scoreData)
        {
            var result = 0;
            result += (430 - ((198 - scoreData.SaveSlot.AetherpoolArm - scoreData.SaveSlot.AetherpoolArmor) * 10)) * (scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber);
            result += (int)(scoreData.CurrentFloorNumber - (scoreData.StartingFloorNumber + Math.Truncate((scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber) / 10.0))) * 50 * 91;

            if (scoreData.SaveSlot.CurrentLevel > 61)
            {
                if (scoreData.CurrentFloorNumber == 100)
                    result += 50 * scoreData.Duty;
            }
            else
            {
                if (scoreData.CurrentFloorNumber == 200)
                    result += 50 * scoreData.Duty;
            }

            result += (int)Math.Truncate((scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber) / 10.0) * scoreData.Duty * 300;
            result += Math.Truncate(scoreData.CurrentFloorNumber / 10.0) == (scoreData.CurrentFloorNumber / 10.0) && scoreData.Duty == 101 && (scoreData.SaveSlot.CurrentLevel > 61 ? scoreData.CurrentFloorNumber != 100 : scoreData.CurrentFloorNumber != 200) ? scoreData.Duty * 300 : 0;

            if (scoreData.SaveSlot.CurrentLevel > 61)
            {
                var res1 = Math.Truncate(scoreData.CurrentFloorNumber / 10.0) == (scoreData.CurrentFloorNumber / 10.0) && scoreData.Duty == 101 ? scoreData.Duty * 300 : 0;
                if (((Math.Truncate((scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber) / 10.0) * scoreData.Duty * 300 + res1) / (scoreData.Duty * 300)) >= 3 - Math.Truncate(scoreData.StartingFloorNumber / 10.0))
                    result += 450 * scoreData.Duty;

                var res2 = Math.Truncate(scoreData.CurrentFloorNumber / 10.0) == (scoreData.CurrentFloorNumber / 10.0) && scoreData.Duty == 101 ? scoreData.Duty * 300 : 0;
                if (((Math.Truncate((scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber) / 10.0) * scoreData.Duty * 300 + res2) / scoreData.Duty * 300) >= 1)
                    result += -50 * scoreData.Duty;

                if (scoreData.CurrentFloorNumber > 60 || (scoreData.CurrentFloorNumber == 60 && scoreData.Duty == 101))
                    result += 50 * scoreData.Duty;

                if (scoreData.CurrentFloorNumber > 70 || (scoreData.CurrentFloorNumber == 70 && scoreData.Duty == 101))
                    result += -50 * scoreData.Duty;
            }
            else
            {
                if (scoreData.StartingFloorNumber == 1)
                    result += -scoreData.Duty * 50 * (int)Math.Min(Math.Truncate(scoreData.CurrentFloorNumber / 10.0), 1.0);

                if (scoreData.StartingFloorNumber == 1)
                {
                    if (scoreData.CurrentFloorNumber > 50 || (scoreData.CurrentFloorNumber == 50 && scoreData.Duty == 101))
                        result += scoreData.Duty * 450;
                }

                if (scoreData.CurrentFloorNumber > 100 || (scoreData.CurrentFloorNumber == 100 && scoreData.Duty == 101))
                    result += scoreData.Duty * 450;

                if (scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber + 1 == 50 && scoreData.Duty == 101)
                    result += -2000;

                if (scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber + 1 == 200 && scoreData.Duty == 101)
                    result += -9500 + 3200 * scoreData.Duty;

                if (scoreData.CurrentFloorNumber == 200 && scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber + 1 == 150 && scoreData.Duty == 101)
                    result += -7000 + 3200 * scoreData.Duty;
            }

            if (scoreData.SaveSlot.CurrentLevel > 61)
            {
                if (scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber + 1 == 30 && scoreData.Duty == 101)
                    result += -1000;
            }

            if (scoreData.SaveSlot.CurrentLevel < 61)
            {
                if (scoreData.CurrentFloorNumber > 60 || (scoreData.Duty == 101 && scoreData.CurrentFloorNumber == 60))
                {
                    if (scoreData.StartingFloorNumber == 51)
                    {
                        result += -50 * scoreData.Duty;
                    }
                    else
                    {
                        if (scoreData.CurrentFloorNumber < 100)
                            result += -50 * scoreData.Duty;
                    }
                }
            }

            if (scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber + 1 == 100 && scoreData.Duty == 101)
            {
                result += -4500;
                if (scoreData.SaveSlot.CurrentLevel > 61)
                {
                    result += 3200 * scoreData.Duty;
                }
            }
            return result;
        }

        private static int MapScore(ScoreData scoreData, int baseScore)
        {
            var result = 0;
            if (baseScore == 0)
            {
                var maps = scoreData.SaveSlot.Maps();
                if ((scoreData.CurrentFloorNumber - scoreData.StartingFloorNumber + 1) > 10)
                    result += scoreData.Duty * maps * 25;
                else
                {
                    if (scoreData.Duty == 101)
                        result += scoreData.Duty * maps * 25;
                    else
                        result += scoreData.Duty * (maps - 2) * 25;
                }
            }
            return result;
        }

        private static int CofferScore(ScoreData scoreData, int baseScore) => baseScore == 0 ? scoreData.SaveSlot.Coffers() * scoreData.Duty : 0;

        private static int NPCScore(ScoreData scoreData, int baseScore) => baseScore == 0 ? scoreData.SaveSlot.NPCs() * scoreData.Duty * 20 : 0;

        private static int MimicMandragoraScore(ScoreData scoreData, int baseScore) => baseScore == 0 ? (scoreData.SaveSlot.Mimics() + scoreData.SaveSlot.Mandragoras()) * scoreData.Duty * 5 : 0;

        private static int EnchantmentScore(ScoreData scoreData, int baseScore) => baseScore == 0 ? scoreData.SaveSlot.Enchantments() * scoreData.Duty * 5 : 0;

        private static int TrapScore(ScoreData scoreData, int baseScore) => baseScore == 0 ? -scoreData.SaveSlot.Traps() * scoreData.Duty * 2 : 0;

        private static int TimeBonusScore(ScoreData scoreData) => scoreData.SaveSlot.TimeBonuses() * scoreData.Duty * 150;

        private static int DeathScore(ScoreData scoreData, int baseScore) => baseScore == 0 ? -scoreData.SaveSlot.Deaths() * scoreData.Duty * 50 : 0;

        private static int KillScore(ScoreData scoreData)
        {
            var floors = scoreData.SaveSlot.FloorSets.SelectMany(x => x.Floors);
            var normalFloors = floors.Where(scoreData.IsNormalFloor);
            var bonusFloors = floors.Where(scoreData.IsBonusFloor);

            var killsBonusException = bonusFloors.Sum(x => x.Mimics + x.Mandragoras) + bonusFloors.Sum(x => x.NPCs) + bonusFloors.Where(x => x.IsLastFloor()).Sum(x => x.Kills);
            var kills = normalFloors.Sum(x => x.Kills) + killsBonusException;
            var killsBonus = bonusFloors.Sum(x => x.Kills) - killsBonusException;

            var floorBonus = (int)(Math.Truncate(scoreData.TotalReachedFloors / 2.0) * scoreData.KillScoreMultiplier()) + 100;
            return (floorBonus * kills) + ((floorBonus + scoreData.Duty) * killsBonus);
        }

        private static ScoreData? CreateScoreData(SaveSlot saveSlot, bool isDutyComplete, DeepDungeon deepDungeon)
        {
            if (deepDungeon == DeepDungeon.ThePalaceOfTheDead)
                return new ScoreDataThePalaceOfTheDead(saveSlot, isDutyComplete);
            else if (deepDungeon == DeepDungeon.HeavenOnHigh)
                return new ScoreDataHeavenOnHigh(saveSlot, isDutyComplete);
            else if (deepDungeon == DeepDungeon.EurekaOrthos)
                return new ScoreDataEurekaOrthos(saveSlot, isDutyComplete);
            return null;
        }

        public static int Calculate(SaveSlot saveSlot, bool isDutyComplete, DeepDungeon deepDungeon, bool showLog = false)
        {
            var scoreData = Score.CreateScoreData(saveSlot, isDutyComplete, deepDungeon);

            if (scoreData == null || !scoreData.IsValidStartingFloor() || !ServiceUtility.IsSolo)
                return 0;

            var baseScore = Score.BaseScore(scoreData);
            var characterScore = Score.CharacterScore(scoreData);
            var floorScore = Score.FloorScore(scoreData);
            var mapScore = Score.MapScore(scoreData, baseScore);
            var cofferScore = Score.CofferScore(scoreData, baseScore);
            var npcScore = Score.NPCScore(scoreData, baseScore);
            var mimicMandragoraScore = Score.MimicMandragoraScore(scoreData, baseScore);
            var enchantmentScore = Score.EnchantmentScore(scoreData, baseScore);
            var trapScore = Score.TrapScore(scoreData, baseScore);
            var timeBonusScore = Score.TimeBonusScore(scoreData);
            var deathScore = Score.DeathScore(scoreData, baseScore);
            var nonKillScore = (mapScore + cofferScore + npcScore + mimicMandragoraScore + enchantmentScore + trapScore + timeBonusScore + deathScore) / scoreData.Duty;

            if (nonKillScore > 0)
                nonKillScore = characterScore + floorScore + mapScore + cofferScore + npcScore + mimicMandragoraScore + enchantmentScore + trapScore + timeBonusScore + deathScore;
            else
                nonKillScore = characterScore + floorScore + baseScore;

            var killScore = Score.KillScore(scoreData);
            var score = nonKillScore + killScore;

            if (showLog && !Score.ShowLogOnce)
            {
                Action<string> p = Log.Print;

                Score.ShowLogOnce = true;
                p(string.Empty);
                p($"[{(isDutyComplete ? "Duty Complete" : "Duty Failed")}] ({baseScore})");
                p($"Current Level: {saveSlot.CurrentLevel} (+{saveSlot.AetherpoolArm}/+{saveSlot.AetherpoolArmor})");
                p($"Current Floor: {scoreData.StartingFloorNumber}->{scoreData.CurrentFloorNumber} ({scoreData.TotalReachedFloors})");
                p(string.Empty);
                p($"Character Score: {characterScore}");
                p($"Floor Score: {floorScore}");
                p($"Map Score: {mapScore}");
                p($"Coffer Score: {cofferScore}");
                p($"NPC Score: {npcScore}");
                p($"Mimic/Mandragora Score: {mimicMandragoraScore}");
                p($"Enchantment Score: {enchantmentScore}");
                p($"Trap Score: {trapScore}");
                p($"Time Bonus Score: {timeBonusScore}");
                p($"Death Score: {deathScore}");
                p($"Non-Kill Score: {nonKillScore}");
                p($"Kill Score: {killScore}");
                p(string.Empty);
                p($"Total Score: {score}");
            }
            return score;
        }
    }
}