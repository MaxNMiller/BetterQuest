using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Progression;
using Spaa.Save;

namespace Spaa.Tests
{
    public class StreakCalculatorTests
    {
        private static DayRecord Day(string key, int completed, int seed = 0)
        {
            var record = new DayRecord { dayKey = key, monsterSeed = seed };
            record.completedPerElement[0] = completed;
            return record;
        }

        [Test]
        public void EmptyHistory_IsZero()
        {
            var history = new List<DayRecord>();

            Assert.AreEqual(0, StreakCalculator.Current(history, "2026-09-27"));
            Assert.AreEqual(0, StreakCalculator.Best(history));
        }

        [Test]
        public void SingleActiveToday_IsOne()
        {
            var history = new List<DayRecord> { Day("2026-09-27", 2) };

            Assert.AreEqual(1, StreakCalculator.Current(history, "2026-09-27"));
            Assert.AreEqual(1, StreakCalculator.Best(history));
        }

        [Test]
        public void ConsecutiveDays_Count()
        {
            var history = new List<DayRecord> { Day("2026-09-25", 1), Day("2026-09-26", 3), Day("2026-09-27", 1) };

            Assert.AreEqual(3, StreakCalculator.Current(history, "2026-09-27"));
        }

        [Test]
        public void CalendarGap_BreaksStreak()
        {
            var history = new List<DayRecord> { Day("2026-09-22", 1), Day("2026-09-23", 1), Day("2026-09-26", 1), Day("2026-09-27", 1) };

            Assert.AreEqual(2, StreakCalculator.Current(history, "2026-09-27"));
            Assert.AreEqual(2, StreakCalculator.Best(history));
        }

        [Test]
        public void DayWithNoHabits_BreaksStreak()
        {
            var history = new List<DayRecord> { Day("2026-09-24", 1), Day("2026-09-25", 1), Day("2026-09-26", 0), Day("2026-09-27", 1) };

            Assert.AreEqual(1, StreakCalculator.Current(history, "2026-09-27"));
            Assert.AreEqual(2, StreakCalculator.Best(history));
        }

        [Test]
        public void SameDateRecords_CountAsOneCalendarDay()
        {
            var history = new List<DayRecord> { Day("2026-09-26", 1, 1), Day("2026-09-27", 1, 2), Day("2026-09-27", 1, 3), Day("2026-09-27", 2, 4) };

            Assert.AreEqual(2, StreakCalculator.Current(history, "2026-09-27"));
            Assert.AreEqual(2, StreakCalculator.Best(history));
        }

        [Test]
        public void SameDate_EmptyRecordThenActiveRecord_DayIsActive()
        {
            var history = new List<DayRecord> { Day("2026-09-25", 1, 1), Day("2026-09-26", 0, 2), Day("2026-09-26", 3, 3), Day("2026-09-27", 1, 4) };

            Assert.AreEqual(3, StreakCalculator.Current(history, "2026-09-27"));
            Assert.AreEqual(3, StreakCalculator.Best(history));
        }

        [Test]
        public void TodayInProgressWithNoHabits_DoesNotBreakYet()
        {
            var history = new List<DayRecord> { Day("2026-09-25", 1), Day("2026-09-26", 1), Day("2026-09-27", 0) };

            Assert.AreEqual(2, StreakCalculator.Current(history, "2026-09-27"));
        }

        [Test]
        public void NoRecordToday_YesterdayActive_KeepsStreak()
        {
            var history = new List<DayRecord> { Day("2026-09-25", 1), Day("2026-09-26", 1) };

            Assert.AreEqual(2, StreakCalculator.Current(history, "2026-09-27"));
        }

        [Test]
        public void LastPlayedDaysAgo_CurrentIsZero_BestKept()
        {
            var history = new List<DayRecord> { Day("2026-09-20", 1), Day("2026-09-21", 1) };

            Assert.AreEqual(0, StreakCalculator.Current(history, "2026-09-27"));
            Assert.AreEqual(2, StreakCalculator.Best(history));
        }

        [Test]
        public void Best_IsAtLeastCurrent()
        {
            var history = new List<DayRecord> { Day("2026-09-20", 1), Day("2026-09-26", 1), Day("2026-09-27", 1) };

            Assert.GreaterOrEqual(StreakCalculator.Best(history), StreakCalculator.Current(history, "2026-09-27"));
        }
    }
}
