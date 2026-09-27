using System;
using System.Globalization;

namespace Spaa.DayCycle
{
    public class DebugDayClock : IDayClock
    {
        private const string DayKeyFormat = "yyyy-MM-dd";
        private string currentDayKey;

        public DebugDayClock(string initialDayKey)
        {
            currentDayKey = initialDayKey;
        }

        public string GetCurrentDayKey()
        {
            return currentDayKey;
        }

        public void SetDayKey(string dayKey)
        {
            currentDayKey = dayKey;
        }

        public void AdvanceDay()
        {
            var date = DateTime.ParseExact(currentDayKey, DayKeyFormat, CultureInfo.InvariantCulture);
            currentDayKey = date.AddDays(1).ToString(DayKeyFormat);
        }
    }
}
