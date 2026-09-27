namespace Spaa.Tutorial
{
    public static class TutorialResume
    {
        public static TutorialStepData ResolveStart(TutorialSequencer sequencer, TutorialProgress progress, TutorialScene scene)
        {
            if (sequencer == null || progress == null || progress.completed || sequencer.Steps.Count == 0)
            {
                return null;
            }

            int index = sequencer.IndexOf(progress.currentStepId);
            if (index < 0)
            {
                var first = sequencer.Steps[0];
                return first.Scene == scene ? first : null;
            }

            for (int i = index; i >= 0; i--)
            {
                var step = sequencer.Steps[i];
                if (step == null || step.Scene != scene)
                {
                    continue;
                }

                var resume = string.IsNullOrEmpty(step.ResumeStepId) ? null : sequencer.Find(step.ResumeStepId);
                return resume ?? step;
            }

            return null;
        }
    }
}
