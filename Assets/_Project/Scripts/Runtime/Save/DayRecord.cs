using System;

namespace Spaa.Save
{
    [Serializable]
    public class DayRecord
    {
        public const int ElementCount = 5;

        public string dayKey = string.Empty;
        public int monsterSeed;
        public int weakness;
        public int monsterMaxHp;
        public bool defeated;
        public int[] completedPerElement = new int[ElementCount];
        public int[] expectedPerElement = new int[ElementCount];
        public int[] damagePerElement = new int[ElementCount];
        public int totalHits;
        public int weaknessHits;
        public int biggestHit;
        public int endOfDayDamageTaken;

        public int TotalDamage => Sum(damagePerElement);
        public int TotalCompleted => Sum(completedPerElement);
        public int TotalExpected => Sum(expectedPerElement);

        private static int Sum(int[] values)
        {
            int total = 0;
            if (values != null)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    total += values[i];
                }
            }

            return total;
        }
    }
}
