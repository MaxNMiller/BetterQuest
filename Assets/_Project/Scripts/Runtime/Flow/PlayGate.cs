using Spaa.Habits;
using Spaa.Save;

namespace Spaa.Flow
{
    public static class PlayGate
    {
        public static PlayBlockReason Evaluate(SaveData save, string todayKey)
        {
            if (IsTodaysMonsterDefeated(save, todayKey) && !save.levelUpPending)
            {
                return PlayBlockReason.MonsterDefeatedToday;
            }

            return HabitRoster.CanPlay(save.customHabits) ? PlayBlockReason.None : PlayBlockReason.MissingHabits;
        }

        public static bool IsTodaysMonsterDefeated(SaveData save, string todayKey)
        {
            return save.currentDayKey == todayKey && save.monsterSeed != 0 && save.monsterHp <= 0;
        }
    }
}
