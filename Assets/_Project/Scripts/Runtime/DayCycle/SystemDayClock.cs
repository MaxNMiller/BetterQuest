using System;

namespace Spaa.DayCycle
{
    public class SystemDayClock : IDayClock
    {
        public string GetCurrentDayKey()
        {
            return DateTime.Now.ToString("yyyy-MM-dd");
        }
    }
}
