using System.Collections.Generic;
using Spaa.Elements;
using Spaa.Save;

namespace Spaa.Progression
{
    public class DashboardStats
    {
        public bool IsEmpty => DaysPlayed == 0;

        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }

        public int DaysPlayed { get; set; }
        public int MonstersDefeated { get; set; }
        public float WinRate { get; set; }

        public int TotalDamage { get; set; }
        public int TodayDamage { get; set; }
        public float AverageDamagePerDay { get; set; }
        public int BiggestHit { get; set; }
        public int TotalHits { get; set; }
        public int WeaknessHits { get; set; }
        public float SuperEffectiveRatio { get; set; }
        public int BestDayDamage { get; set; }
        public string BestDayKey { get; set; } = string.Empty;
        public int EndOfDayHitsTaken { get; set; }

        public int TotalCompleted { get; set; }
        public int[] CompletedPerElement { get; } = new int[DayRecord.ElementCount];
        public int[] DamagePerElement { get; } = new int[DayRecord.ElementCount];
        public int[] ExpectedPerElement { get; } = new int[DayRecord.ElementCount];
        public float[] ShareOfTotal { get; } = new float[DayRecord.ElementCount];
        public float[] CompletionRate { get; } = new float[DayRecord.ElementCount];
        public Element? FavouriteElement { get; set; }
        public Element? NeglectedElement { get; set; }

        public IReadOnlyList<DashboardDay> RecentDays { get; set; } = new List<DashboardDay>();
    }
}
