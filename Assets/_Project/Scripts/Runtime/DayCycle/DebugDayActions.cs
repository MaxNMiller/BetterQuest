using Spaa.Save;

namespace Spaa.DayCycle
{
    public static class DebugDayActions
    {
        public static void StartFreshDay(SaveData save)
        {
            if (save.monsterSeed != 0)
            {
                save.lastMonsterSeed = save.monsterSeed;
            }

            save.currentDayKey = string.Empty;
            save.monsterSeed = 0;
            save.monsterHp = 0;
            save.levelUpPending = false;
            save.completedHabitIds.Clear();
        }
    }
}
