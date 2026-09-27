using Spaa.Save;

namespace Spaa.Tutorial
{
    public static class TutorialEligibility
    {
        public static TutorialStartDecision Evaluate(TutorialProgress progress, SaveData save, bool enabled,
            int currentVersion, bool replayOnVersionBump)
        {
            if (!enabled)
            {
                return TutorialStartDecision.Disabled;
            }

            if (progress == null)
            {
                return IsFreshSave(save) ? TutorialStartDecision.Start : TutorialStartDecision.MarkCompleteSilently;
            }

            if (progress.completed)
            {
                return replayOnVersionBump && progress.version < currentVersion
                    ? TutorialStartDecision.Replay
                    : TutorialStartDecision.None;
            }

            return TutorialStartDecision.Resume;
        }

        public static bool IsFreshSave(SaveData save)
        {
            return save == null
                || ((save.history == null || save.history.Count == 0) && string.IsNullOrEmpty(save.currentDayKey));
        }
    }
}
