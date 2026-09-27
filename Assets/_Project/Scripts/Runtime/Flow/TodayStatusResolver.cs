using Spaa.Battle;
using Spaa.Habits;
using Spaa.Save;

namespace Spaa.Flow
{
    public static class TodayStatusResolver
    {
        public static TodayStatus Resolve(SaveData save, string todayKey)
        {
            var reason = PlayGate.Evaluate(save, todayKey);
            if (reason == PlayBlockReason.MonsterDefeatedToday)
            {
                return new TodayStatus(TodayStatusKind.Defeated, null, null, null);
            }

            if (reason == PlayBlockReason.MissingHabits)
            {
                return new TodayStatus(TodayStatusKind.MissingHabits, null, null, HabitRoster.MissingElements(save.customHabits));
            }

            bool startedToday = save.currentDayKey == todayKey && save.monsterSeed != 0;
            if (!startedToday)
            {
                return new TodayStatus(TodayStatusKind.Fresh, null, null, null);
            }

            var monster = DailyMonsterGenerator.Generate(save.monsterSeed, 0);
            var kind = save.monsterHp <= 0 ? TodayStatusKind.LevelUpPending : TodayStatusKind.InProgress;
            return new TodayStatus(kind, monster.Weakness, monster.Name, null);
        }
    }
}
