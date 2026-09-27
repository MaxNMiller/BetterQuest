namespace Spaa.Progression
{
    public readonly struct DashboardDay
    {
        public string DayKey { get; }
        public int Weakness { get; }
        public DashboardDayStatus Status { get; }
        public int Damage { get; }
        public int Completed { get; }

        public DashboardDay(string dayKey, int weakness, DashboardDayStatus status, int damage, int completed)
        {
            DayKey = dayKey;
            Weakness = weakness;
            Status = status;
            Damage = damage;
            Completed = completed;
        }
    }
}
