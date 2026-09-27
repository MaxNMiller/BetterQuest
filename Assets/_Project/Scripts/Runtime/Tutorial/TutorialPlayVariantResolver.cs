using Spaa.Flow;
using Spaa.Save;

namespace Spaa.Tutorial
{
    public static class TutorialPlayVariantResolver
    {
        public static TutorialPlayVariant Resolve(SaveData save, string todayKey)
        {
            switch (TodayStatusResolver.Resolve(save, todayKey).Kind)
            {
                case TodayStatusKind.MissingHabits:
                    return TutorialPlayVariant.MissingHabits;
                case TodayStatusKind.Defeated:
                    return TutorialPlayVariant.Defeated;
                case TodayStatusKind.LevelUpPending:
                    return TutorialPlayVariant.LevelUpPending;
                default:
                    return TutorialPlayVariant.Ready;
            }
        }
    }
}
