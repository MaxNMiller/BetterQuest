using System;
using System.Collections.Generic;
using Spaa.Progression;
using Spaa.Save;

namespace Spaa.DebugTools
{
    public static class DemoHistoryGenerator
    {
        public const int DefaultSeed = 2026;
        public const int PastDayCount = 14;

        private const int SkippedDaysAgo = 6;
        private const int SeedBase = -100000;

        public static List<DayRecord> Generate(string todayKey, int seed)
        {
            var random = new Random(seed);
            var days = new List<DayRecord>();

            for (int daysAgo = PastDayCount; daysAgo >= 1; daysAgo--)
            {
                if (daysAgo == SkippedDaysAgo)
                {
                    continue;
                }

                days.Add(GenerateDay(random, DayKeyMath.AddDays(todayKey, -daysAgo), SeedBase - daysAgo));
            }

            return days;
        }

        public static void ReplaceHistory(List<DayRecord> history, string todayKey, int seed)
        {
            var todays = new List<DayRecord>();
            foreach (var record in history)
            {
                if (record != null && record.dayKey == todayKey)
                {
                    todays.Add(record);
                }
            }

            history.Clear();
            history.AddRange(Generate(todayKey, seed));
            history.AddRange(todays);
        }

        private static DayRecord GenerateDay(Random random, string dayKey, int monsterSeed)
        {
            int weakness = random.Next(DayRecord.ElementCount);
            var record = new DayRecord { dayKey = dayKey, monsterSeed = monsterSeed, weakness = weakness };

            int expectedDamage = 0;
            for (int e = 0; e < DayRecord.ElementCount; e++)
            {
                int expected = random.Next(1, 4);
                record.expectedPerElement[e] = expected;
                expectedDamage += expected * 10;

                int completed = random.Next(0, 5) == 0 ? 0 : random.Next(1, expected + 1);
                record.completedPerElement[e] = completed;

                for (int hit = 0; hit < completed; hit++)
                {
                    bool weak = e == weakness;
                    int damage = random.Next(10, 16) * (weak ? 2 : 1);
                    record.damagePerElement[e] += damage;
                    record.totalHits++;
                    record.biggestHit = Math.Max(record.biggestHit, damage);
                    if (weak)
                    {
                        record.weaknessHits++;
                    }
                }
            }

            if (record.totalHits == 0)
            {
                record.completedPerElement[weakness] = 1;
                record.damagePerElement[weakness] = 24;
                record.totalHits = 1;
                record.weaknessHits = 1;
                record.biggestHit = 24;
            }

            record.monsterMaxHp = Math.Max(1, (int)Math.Ceiling(0.7 * expectedDamage));
            record.defeated = record.TotalDamage >= record.monsterMaxHp;
            if (!record.defeated)
            {
                record.endOfDayDamageTaken = Math.Max(1, record.monsterMaxHp / 10);
            }

            return record;
        }
    }
}
