using System;
using System.Globalization;

namespace Spaa.Progression
{
    public static class DayKeyMath
    {
        private const string Format = "yyyy-MM-dd";

        public static int DaysBetween(string fromKey, string toKey)
        {
            if (TryParse(fromKey, out var from) && TryParse(toKey, out var to))
            {
                return (int)Math.Round((to - from).TotalDays);
            }

            return 0;
        }

        public static string AddDays(string dayKey, int days)
        {
            return TryParse(dayKey, out var date)
                ? date.AddDays(days).ToString(Format, CultureInfo.InvariantCulture)
                : dayKey;
        }

        private static bool TryParse(string dayKey, out DateTime date)
        {
            return DateTime.TryParseExact(dayKey, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
        }
    }
}
