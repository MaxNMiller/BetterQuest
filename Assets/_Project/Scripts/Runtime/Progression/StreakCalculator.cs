using System.Collections.Generic;
using Spaa.Save;

namespace Spaa.Progression
{
    public static class StreakCalculator
    {
        public static int Current(IReadOnlyList<DayRecord> history, string todayKey)
        {
            var completedByDate = CompletedByDate(history);
            if (completedByDate.Count == 0)
            {
                return 0;
            }

            int streak = 0;
            for (int back = 0; back <= completedByDate.Count; back++)
            {
                string dayKey = DayKeyMath.AddDays(todayKey, -back);
                completedByDate.TryGetValue(dayKey, out int completed);
                if (completed > 0)
                {
                    streak++;
                }
                else if (back > 0)
                {
                    break;
                }

            }

            return streak;
        }

        public static int Best(IReadOnlyList<DayRecord> history)
        {
            if (history == null)
            {
                return 0;
            }

            int best = 0;
            int run = 0;
            string runEndKey = null;
            var completedByDate = CompletedByDate(history);
            var seen = new HashSet<string>();
            for (int i = 0; i < history.Count; i++)
            {
                var record = history[i];
                string dayKey = record == null ? null : record.dayKey ?? string.Empty;
                if (dayKey == null || !seen.Add(dayKey))
                {
                    continue;
                }

                if (completedByDate[dayKey] == 0)
                {
                    run = 0;
                    continue;
                }

                bool continues = run > 0 && DayKeyMath.DaysBetween(runEndKey, dayKey) == 1;
                run = continues ? run + 1 : 1;
                runEndKey = dayKey;
                if (run > best)
                {
                    best = run;
                }
            }

            return best;
        }

        private static Dictionary<string, int> CompletedByDate(IReadOnlyList<DayRecord> history)
        {
            var completedByDate = new Dictionary<string, int>();
            if (history == null)
            {
                return completedByDate;
            }

            for (int i = 0; i < history.Count; i++)
            {
                var record = history[i];
                if (record == null)
                {
                    continue;
                }

                completedByDate.TryGetValue(record.dayKey ?? string.Empty, out int completed);
                completedByDate[record.dayKey ?? string.Empty] = completed + record.TotalCompleted;
            }

            return completedByDate;
        }
    }
}
