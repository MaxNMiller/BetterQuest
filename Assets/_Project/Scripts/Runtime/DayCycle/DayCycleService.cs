namespace Spaa.DayCycle
{
    public class DayCycleService
    {
        private readonly IDayClock clock;

        public DayCycleService(IDayClock clock)
        {
            this.clock = clock;
        }

        public DayRolloverOutcome CheckRollover(string lastDayKey, bool monsterDefeated)
        {
            string currentDayKey = clock.GetCurrentDayKey();
            if (currentDayKey == lastDayKey)
            {
                return DayRolloverOutcome.NoRollover;
            }

            return monsterDefeated ? DayRolloverOutcome.NewDayFresh : DayRolloverOutcome.NewDayMonsterSurvived;
        }
    }
}
