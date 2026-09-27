using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Elements;
using Spaa.Progression;
using Spaa.Save;

namespace Spaa.Tests
{
    public class DashboardStatsCalculatorTests
    {
        private static readonly int[] Expected = { 2, 2, 4, 2, 2 };

        private static List<DayRecord> TwoDays()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            recorder.BeginDay("2026-09-26", 1, Element.Food, 60, Expected);
            recorder.RecordHit(1, Element.Food, 20, true);
            recorder.RecordHit(1, Element.Food, 20, true);
            recorder.RecordHit(1, Element.Rest, 10, false);
            recorder.RecordHit(1, Element.Move, 10, false);
            recorder.RecordDefeat(1);
            recorder.BeginDay("2026-09-27", 2, Element.Move, 60, Expected);
            recorder.RecordHit(2, Element.Move, 30, true);
            return history;
        }

        [Test]
        public void EmptyHistory_IsEmptyAndSafe()
        {
            var stats = DashboardStatsCalculator.Compute(new List<DayRecord>(), "2026-09-27");

            Assert.IsTrue(stats.IsEmpty);
            Assert.AreEqual(0, stats.TotalDamage);
            Assert.AreEqual(0f, stats.SuperEffectiveRatio);
            Assert.AreEqual(0f, stats.WinRate);
            Assert.IsNull(stats.FavouriteElement);
            Assert.IsNull(stats.NeglectedElement);
            for (int i = 0; i < DayRecord.ElementCount; i++)
            {
                Assert.AreEqual(0f, stats.ShareOfTotal[i]);
            }
        }

        [Test]
        public void Totals_AcrossDays()
        {
            var stats = DashboardStatsCalculator.Compute(TwoDays(), "2026-09-27");

            Assert.AreEqual(90, stats.TotalDamage);
            Assert.AreEqual(30, stats.TodayDamage);
            Assert.AreEqual(45f, stats.AverageDamagePerDay, 0.001f);
            Assert.AreEqual(30, stats.BiggestHit);
            Assert.AreEqual(5, stats.TotalCompleted);
            Assert.AreEqual(2, stats.DaysPlayed);
            Assert.AreEqual(1, stats.MonstersDefeated);
            Assert.AreEqual(0.5f, stats.WinRate, 0.001f);
            Assert.AreEqual(3f / 5f, stats.SuperEffectiveRatio, 0.001f);
            Assert.AreEqual(2, stats.CurrentStreak);
            Assert.AreEqual("2026-09-26", stats.BestDayKey);
            Assert.AreEqual(60, stats.BestDayDamage);
        }

        [Test]
        public void PerElement_CountsSharesAndCompletionRates()
        {
            var stats = DashboardStatsCalculator.Compute(TwoDays(), "2026-09-27");

            Assert.AreEqual(2, stats.CompletedPerElement[(int)Element.Food]);
            Assert.AreEqual(40, stats.DamagePerElement[(int)Element.Food]);
            Assert.AreEqual(2, stats.CompletedPerElement[(int)Element.Move]);

            float sum = 0f;
            for (int i = 0; i < DayRecord.ElementCount; i++)
            {
                sum += stats.ShareOfTotal[i];
            }

            Assert.AreEqual(1f, sum, 0.001f);
            Assert.AreEqual(0.4f, stats.ShareOfTotal[(int)Element.Food], 0.001f);
            Assert.AreEqual(0.25f, stats.CompletionRate[(int)Element.Food], 0.001f);
            Assert.AreEqual(0f, stats.CompletionRate[(int)Element.SelfCare], 0.001f);
        }

        [Test]
        public void FavouriteAndNeglected_TiesResolveInEnumOrder()
        {
            var stats = DashboardStatsCalculator.Compute(TwoDays(), "2026-09-27");

            Assert.AreEqual(Element.Food, stats.FavouriteElement);
            Assert.AreEqual(Element.SelfCare, stats.NeglectedElement);
        }

        [Test]
        public void Neglected_WhenAllDone_IsOldestLastCompletion()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            recorder.BeginDay("2026-09-26", 1, Element.Food, 60, Expected);
            recorder.RecordHit(1, Element.Rebuild, 10, false);
            recorder.BeginDay("2026-09-27", 2, Element.Food, 60, Expected);
            recorder.RecordHit(2, Element.Rest, 10, false);
            recorder.RecordHit(2, Element.SelfCare, 10, false);
            recorder.RecordHit(2, Element.Food, 10, true);
            recorder.RecordHit(2, Element.Move, 10, false);

            var stats = DashboardStatsCalculator.Compute(history, "2026-09-27");

            Assert.AreEqual(Element.Rebuild, stats.NeglectedElement);
        }

        [Test]
        public void RecentDays_StatusesAndGapPadding()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            recorder.BeginDay("2026-09-23", 1, Element.Food, 60, Expected);
            recorder.RecordHit(1, Element.Food, 20, true);
            recorder.RecordDefeat(1);
            recorder.BeginDay("2026-09-24", 2, Element.Rest, 60, Expected);
            recorder.RecordHit(2, Element.Rest, 20, true);
            recorder.BeginDay("2026-09-27", 3, Element.Move, 60, Expected);

            var days = DashboardStatsCalculator.RecentDays(history, "2026-09-27", 5);

            Assert.AreEqual(5, days.Count);
            Assert.AreEqual("2026-09-23", days[0].DayKey);
            Assert.AreEqual(DashboardDayStatus.Defeated, days[0].Status);
            Assert.AreEqual((int)Element.Food, days[0].Weakness);
            Assert.AreEqual(DashboardDayStatus.Partial, days[1].Status);
            Assert.AreEqual(DashboardDayStatus.Missed, days[2].Status);
            Assert.AreEqual("2026-09-25", days[2].DayKey);
            Assert.AreEqual(-1, days[2].Weakness);
            Assert.AreEqual(DashboardDayStatus.Missed, days[3].Status);
            Assert.AreEqual(DashboardDayStatus.InProgress, days[4].Status);
        }

        [Test]
        public void RecentDays_NoRecordToday_AddsTodayPlaceholder_AndTrimsToCount()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            for (int i = 0; i < 10; i++)
            {
                recorder.BeginDay($"2026-09-{10 + i:00}", i + 1, Element.Rest, 60, Expected);
                recorder.RecordHit(i + 1, Element.Rest, 10, true);
            }

            var days = DashboardStatsCalculator.RecentDays(history, "2026-09-20", 7);

            Assert.AreEqual(7, days.Count);
            Assert.AreEqual("2026-09-20", days[6].DayKey);
            Assert.AreEqual(DashboardDayStatus.InProgress, days[6].Status);
            Assert.AreEqual("2026-09-19", days[5].DayKey);
        }

        [Test]
        public void RecentDays_SameDateRecords_MergeIntoOneCalendarDay()
        {
            var history = new List<DayRecord>();
            var recorder = new HistoryRecorder(history);
            recorder.BeginDay("2026-09-27", 1, Element.Food, 60, Expected);
            recorder.RecordHit(1, Element.Food, 20, true);
            recorder.BeginDay("2026-09-27", 2, Element.Rest, 60, Expected);
            recorder.RecordHit(2, Element.Rest, 30, true);
            recorder.RecordDefeat(2);
            recorder.BeginDay("2026-09-28", 3, Element.Move, 60, Expected);

            var days = DashboardStatsCalculator.RecentDays(history, "2026-09-28", 7);

            Assert.AreEqual(7, days.Count);
            Assert.AreEqual("2026-09-22", days[0].DayKey);
            Assert.AreEqual("2026-09-27", days[5].DayKey);
            Assert.AreEqual(50, days[5].Damage);
            Assert.AreEqual(2, days[5].Completed);
            Assert.AreEqual(DashboardDayStatus.Defeated, days[5].Status);
            Assert.AreEqual((int)Element.Rest, days[5].Weakness);
            Assert.AreEqual(DashboardDayStatus.InProgress, days[6].Status);
            for (int i = 0; i < days.Count - 1; i++)
            {
                Assert.AreEqual(1, Spaa.Progression.DayKeyMath.DaysBetween(days[i].DayKey, days[i + 1].DayKey));
            }
        }

        [Test]
        public void RecentDays_EmptyHistory_StillReturnsFullWeek()
        {
            var days = DashboardStatsCalculator.RecentDays(new List<DayRecord>(), "2026-09-27", 7);

            Assert.AreEqual(7, days.Count);
            Assert.AreEqual(DashboardDayStatus.Missed, days[0].Status);
            Assert.AreEqual(DashboardDayStatus.InProgress, days[6].Status);
        }
    }
}
