using System.Collections.Generic;
using NUnit.Framework;
using Spaa.DebugTools;
using Spaa.Progression;
using Spaa.Save;

namespace Spaa.Tests
{
    public class DemoHistoryGeneratorTests
    {
        private const string Today = "2026-09-27";

        [Test]
        public void Generate_IsDeterministic()
        {
            var a = DemoHistoryGenerator.Generate(Today, 7);
            var b = DemoHistoryGenerator.Generate(Today, 7);

            Assert.AreEqual(a.Count, b.Count);
            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].dayKey, b[i].dayKey);
                Assert.AreEqual(a[i].TotalDamage, b[i].TotalDamage);
                Assert.AreEqual(a[i].defeated, b[i].defeated);
            }
        }

        [Test]
        public void Generate_CoversPastDaysOnly_WithOneGap()
        {
            var days = DemoHistoryGenerator.Generate(Today, 7);

            Assert.AreEqual(DemoHistoryGenerator.PastDayCount - 1, days.Count);
            Assert.AreEqual("2026-09-13", days[0].dayKey);
            Assert.AreEqual("2026-09-26", days[days.Count - 1].dayKey);
            Assert.Less(StreakCalculator.Best(days), days.Count);
        }

        [Test]
        public void Generate_ValuesAreInRange()
        {
            foreach (var day in DemoHistoryGenerator.Generate(Today, 3))
            {
                Assert.That(day.weakness, Is.InRange(0, DayRecord.ElementCount - 1));
                Assert.Greater(day.TotalCompleted, 0);
                Assert.Greater(day.monsterMaxHp, 0);
                Assert.LessOrEqual(day.weaknessHits, day.totalHits);
                Assert.AreEqual(day.TotalCompleted, day.totalHits);
                Assert.AreEqual(day.defeated, day.TotalDamage >= day.monsterMaxHp);
                for (int e = 0; e < DayRecord.ElementCount; e++)
                {
                    Assert.LessOrEqual(day.completedPerElement[e], day.expectedPerElement[e]);
                }
            }
        }

        [Test]
        public void ReplaceHistory_KeepsTodaysRecordsAtTheEnd()
        {
            var today = new DayRecord { dayKey = Today, monsterSeed = 42 };
            var history = new List<DayRecord> { new DayRecord { dayKey = "2026-01-01", monsterSeed = 1 }, today };

            DemoHistoryGenerator.ReplaceHistory(history, Today, 7);

            Assert.AreSame(today, history[history.Count - 1]);
            Assert.AreEqual(DemoHistoryGenerator.PastDayCount, history.Count);
            Assert.AreEqual("2026-09-13", history[0].dayKey);
            Assert.GreaterOrEqual(StreakCalculator.Current(history, Today), 5);
        }
    }
}
