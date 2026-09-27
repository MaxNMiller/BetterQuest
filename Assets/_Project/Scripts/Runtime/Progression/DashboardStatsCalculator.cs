using System;
using System.Collections.Generic;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.Progression
{
    public static class DashboardStatsCalculator
    {
        public const int RecentDayCount = 14;

        public static DashboardStats Compute(IReadOnlyList<DayRecord> history, string todayKey)
        {
            var stats = new DashboardStats();
            history = history ?? Array.Empty<DayRecord>();

            var lastCompletedIndex = new int[DayRecord.ElementCount];
            for (int e = 0; e < DayRecord.ElementCount; e++)
            {
                lastCompletedIndex[e] = -1;
            }

            for (int i = 0; i < history.Count; i++)
            {
                var record = history[i];
                stats.DaysPlayed++;
                if (record.defeated)
                {
                    stats.MonstersDefeated++;
                }

                int dayDamage = record.TotalDamage;
                stats.TotalDamage += dayDamage;
                stats.TotalHits += record.totalHits;
                stats.WeaknessHits += record.weaknessHits;
                stats.BiggestHit = Math.Max(stats.BiggestHit, record.biggestHit);
                if (record.endOfDayDamageTaken > 0)
                {
                    stats.EndOfDayHitsTaken++;
                }

                if (dayDamage > stats.BestDayDamage)
                {
                    stats.BestDayDamage = dayDamage;
                    stats.BestDayKey = record.dayKey;
                }

                for (int e = 0; e < DayRecord.ElementCount; e++)
                {
                    int completed = At(record.completedPerElement, e);
                    stats.CompletedPerElement[e] += completed;
                    stats.DamagePerElement[e] += At(record.damagePerElement, e);
                    stats.ExpectedPerElement[e] += At(record.expectedPerElement, e);
                    if (completed > 0)
                    {
                        lastCompletedIndex[e] = i;
                    }
                }
            }

            if (history.Count > 0 && history[history.Count - 1].dayKey == todayKey)
            {
                stats.TodayDamage = history[history.Count - 1].TotalDamage;
            }

            for (int e = 0; e < DayRecord.ElementCount; e++)
            {
                stats.TotalCompleted += stats.CompletedPerElement[e];
            }

            for (int e = 0; e < DayRecord.ElementCount; e++)
            {
                stats.ShareOfTotal[e] = Ratio(stats.CompletedPerElement[e], stats.TotalCompleted);
                stats.CompletionRate[e] = Math.Min(1f, Ratio(stats.CompletedPerElement[e], stats.ExpectedPerElement[e]));
            }

            stats.WinRate = Ratio(stats.MonstersDefeated, stats.DaysPlayed);
            stats.SuperEffectiveRatio = Ratio(stats.WeaknessHits, stats.TotalHits);
            stats.AverageDamagePerDay = Ratio(stats.TotalDamage, stats.DaysPlayed);
            stats.FavouriteElement = Favourite(stats.CompletedPerElement);
            stats.NeglectedElement = history.Count > 0 ? Neglected(lastCompletedIndex) : (Element?)null;
            stats.CurrentStreak = StreakCalculator.Current(history, todayKey);
            stats.BestStreak = Math.Max(stats.CurrentStreak, StreakCalculator.Best(history));
            stats.RecentDays = RecentDays(history, todayKey, RecentDayCount);
            return stats;
        }

        public static List<DashboardDay> RecentDays(IReadOnlyList<DayRecord> history, string todayKey, int count)
        {
            var days = new List<DashboardDay>(count);
            history = history ?? Array.Empty<DayRecord>();

            for (int back = count - 1; back >= 0; back--)
            {
                string dayKey = DayKeyMath.AddDays(todayKey, -back);
                int weakness = -1;
                int damage = 0;
                int completed = 0;
                bool found = false;
                bool defeated = false;

                for (int i = 0; i < history.Count; i++)
                {
                    var record = history[i];
                    if (record == null || record.dayKey != dayKey)
                    {
                        continue;
                    }

                    found = true;
                    weakness = record.weakness;
                    damage += record.TotalDamage;
                    completed += record.TotalCompleted;
                    defeated |= record.defeated;
                }

                days.Add(new DashboardDay(dayKey, weakness, StatusOf(found, defeated, completed, back == 0), damage, completed));
            }

            return days;
        }

        private static DashboardDayStatus StatusOf(bool found, bool defeated, int completed, bool isToday)
        {
            if (defeated)
            {
                return DashboardDayStatus.Defeated;
            }

            if (isToday)
            {
                return DashboardDayStatus.InProgress;
            }

            return found && completed > 0 ? DashboardDayStatus.Partial : DashboardDayStatus.Missed;
        }

        private static Element? Favourite(int[] completed)
        {
            int best = -1;
            for (int e = 0; e < completed.Length; e++)
            {
                if (completed[e] > 0 && (best < 0 || completed[e] > completed[best]))
                {
                    best = e;
                }
            }

            return best < 0 ? (Element?)null : (Element)best;
        }

        private static Element Neglected(int[] lastCompletedIndex)
        {
            int oldest = 0;
            for (int e = 1; e < lastCompletedIndex.Length; e++)
            {
                if (lastCompletedIndex[e] < lastCompletedIndex[oldest])
                {
                    oldest = e;
                }
            }

            return (Element)oldest;
        }

        private static int At(int[] values, int index)
        {
            return values != null && index < values.Length ? values[index] : 0;
        }

        private static float Ratio(int part, int whole)
        {
            return whole > 0 ? (float)part / whole : 0f;
        }
    }
}
