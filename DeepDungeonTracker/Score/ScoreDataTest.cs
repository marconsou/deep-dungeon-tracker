namespace DeepDungeonTracker
{
    public static class ScoreDataTest
    {
        private static SaveSlot SaveSlot { get; set; } = null!;

        public static SaveSlot T1()
        {
            ScoreDataTest.SaveSlot = new(DeepDungeon.PalaceOfTheDead, currentLevel: 60);
            ScoreDataTest.SaveSlot.AetherpoolUpdate(99, 99);

            ScoreDataTest.Floors(100, 0, 10);
            ScoreDataTest.Kills(1123, 10, false);
            ScoreDataTest.Mimics(63, false);
            ScoreDataTest.NPCs(3, false);
            ScoreDataTest.Coffers(483);
            ScoreDataTest.Enchantments(34);
            ScoreDataTest.Traps(42);
            ScoreDataTest.Deaths(0);
            ScoreDataTest.Maps(90);
            ScoreDataTest.TimeBonuses(10);

            return ScoreDataTest.SaveSlot;
        }

        private static void Floors(int total, int startingFloorSet = 0, int floorSetTotal = 20)
        {
            var counter = 1;
            var floorTotal = 10;
            for (var y = startingFloorSet; y < startingFloorSet + floorSetTotal; y++)
            {
                ScoreDataTest.SaveSlot.AddFloorSet((y * floorTotal) + 1);
                counter++;
                for (var x = 0; x < floorTotal - 1; x++)
                {
                    if (counter <= total)
                        ScoreDataTest.SaveSlot.AddFloor();
                    else
                        return;
                    counter++;
                }
            }
        }

        private static void Kills(int total, int bosses, bool bonus)
        {
            var floorSet = ScoreDataTest.SaveSlot.FloorSets[!bonus ? 0 : ^1];
            for (var i = 0; i < total - bosses; i++)
                floorSet.Floors[0].EnemyKilled();

            for (var i = 0; i < bosses; i++)
                floorSet.Floors[^1].EnemyKilled();
        }

        private static void Mimics(int total, bool bonus)
        {
            for (var i = 0; i < total; i++)
                ScoreDataTest.SaveSlot.FloorSets[!bonus ? 0 : ^1].Floors[0].MimicKilled();
        }

        private static void NPCs(int total, bool bonus)
        {
            for (var i = 0; i < total; i++)
                ScoreDataTest.SaveSlot.FloorSets[!bonus ? 0 : ^1].Floors[0].NPCKilled();
        }

        private static void Coffers(int total)
        {
            for (var i = 0; i < total; i++)
                ScoreDataTest.SaveSlot.FloorSets[0].Floors[0].CofferOpened(Coffer.PomanderOfSafety);
        }

        private static void Enchantments(int total)
        {
            for (var i = 0; i < total; i++)
                ScoreDataTest.SaveSlot.FloorSets[0].Floors[0].EnchantmentAffected(Enchantment.Blindness);
        }

        private static void Traps(int total)
        {
            for (var i = 0; i < total; i++)
                ScoreDataTest.SaveSlot.FloorSets[0].Floors[0].TrapTriggered(Trap.Landmine);
        }

        private static void Deaths(int total)
        {
            for (var i = 0; i < total; i++)
                ScoreDataTest.SaveSlot.FloorSets[0].Floors[0].PlayerKilled();
        }

        private static void Maps(int total)
        {
            var counter = 1;
            foreach (var floorSet in ScoreDataTest.SaveSlot.FloorSets)
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
                ScoreDataTest.SaveSlot.FloorSets[i].CheckForTimeBonus(new(0, 29, 0));
        }
    }
}