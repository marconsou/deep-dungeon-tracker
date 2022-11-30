using System.Linq;

namespace DeepDungeonTracker
{
    public static class ScoreSampleData
    {
        private static SaveSlot SaveSlot { get; set; } = null!;

        public static SaveSlot T1()
        {
            ScoreSampleData.SaveSlot = new(currentLevel: 60);
            ScoreSampleData.SaveSlot.AetherpoolUpdate(99, 99);

            ScoreSampleData.Floors(100, 0, 10);
            ScoreSampleData.Kills(1123, 10, false);
            ScoreSampleData.Mimics(63, false);
            ScoreSampleData.NPCs(3, false);
            ScoreSampleData.Coffers(483);
            ScoreSampleData.Enchantments(34);
            ScoreSampleData.Traps(42);
            ScoreSampleData.Deaths(0);
            ScoreSampleData.Maps(90);
            ScoreSampleData.TimeBonuses(10);

            return ScoreSampleData.SaveSlot;
        }

        private static void Floors(int total, int startingFloorSet = 0, int floorSetTotal = 20)
        {
            var counter = 1;
            var floorTotal = 10;
            for (var y = startingFloorSet; y < startingFloorSet + floorSetTotal; y++)
            {
                ScoreSampleData.SaveSlot.AddFloorSet((y * floorTotal) + 1);
                counter++;
                for (var x = 0; x < floorTotal - 1; x++)
                {
                    if (counter <= total)
                        ScoreSampleData.SaveSlot.AddFloor();
                    else
                        return;
                    counter++;
                }
            }
        }

        private static void Kills(int total, int bosses, bool bonus)
        {
            var floorSet = ScoreSampleData.SaveSlot.FloorSets[!bonus ? 0 : ^1];
            for (var i = 0; i < total - bosses; i++)
                floorSet.Floors[0].EnemyKilled();

            for (var i = 0; i < bosses; i++)
                floorSet.Floors[^1].EnemyKilled();
        }

        private static void Mimics(int total, bool bonus)
        {
            for (var i = 0; i < total; i++)
                ScoreSampleData.SaveSlot.FloorSets[!bonus ? 0 : ^1].Floors[0].MimicKilled();
        }

        private static void NPCs(int total, bool bonus)
        {
            for (var i = 0; i < total; i++)
                ScoreSampleData.SaveSlot.FloorSets[!bonus ? 0 : ^1].Floors[0].NPCKilled();
        }

        private static void Coffers(int total) => ScoreSampleData.SaveSlot.FloorSets[0].Floors[0].Coffers.AddRange(Enumerable.Repeat(Coffer.PomanderOfSafety, total).ToList());

        private static void Enchantments(int total) => ScoreSampleData.SaveSlot.FloorSets[0].Floors[0].Enchantments.AddRange(Enumerable.Repeat(Enchantment.Blindness, total).ToList());

        private static void Traps(int total) => ScoreSampleData.SaveSlot.FloorSets[0].Floors[0].Traps.AddRange(Enumerable.Repeat(Trap.Landmine, total).ToList());

        private static void Deaths(int total)
        {
            for (var i = 0; i < total; i++)
                ScoreSampleData.SaveSlot.FloorSets[0].Floors[0].PlayerKilled();
        }

        private static void Maps(int total)
        {
            var counter = 1;
            foreach (var floorSet in ScoreSampleData.SaveSlot.FloorSets)
            {
                foreach (var floor in floorSet.Floors)
                {
                    if (counter <= total)
                        floor.MapFullyRevealed();
                    else
                        return;
                    counter++;
                }
            }
        }

        private static void TimeBonuses(int total)
        {
            for (var i = 0; i < total; i++)
                ScoreSampleData.SaveSlot.FloorSets[i].CheckForTimeBonus(new(0, 29, 0));
        }
    }
}