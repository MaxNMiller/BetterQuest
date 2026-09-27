namespace Spaa.Tutorial
{
    public static class TutorialRules
    {
        public static bool CanSkip(TutorialStepData step, TutorialProgress progress, bool fallbackActive)
        {
            if (progress == null || progress.completed)
            {
                return false;
            }

            if (fallbackActive || progress.isReplay || step == null)
            {
                return true;
            }

            return !step.LockSkipUntilFirstHabit || progress.firstHabitWritten;
        }
    }
}
