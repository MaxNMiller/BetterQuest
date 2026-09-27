namespace Spaa.Tutorial
{
    public readonly struct TutorialOverlayModel
    {
        public const string DefaultNextLabel = "Next";
        public const string FinishLabel = "Let's go!";
        public const string DefaultNotYetLabel = "Not yet";
        public const string DefaultGotItLabel = "Got it";

        public bool ShowNext { get; }
        public string NextLabel { get; }
        public bool ShowNotYet { get; }
        public string NotYetLabel { get; }
        public bool ShowSkip { get; }
        public bool ShowLearnMore { get; }
        public bool ShowElements { get; }
        public bool BlockHole { get; }
        public bool Fallback { get; }
        public bool Dim { get; }

        private TutorialOverlayModel(bool showNext, string nextLabel, bool showNotYet, string notYetLabel, bool showSkip,
            bool showLearnMore, bool showElements, bool blockHole, bool fallback, bool dim = true)
        {
            Dim = dim;
            ShowNext = showNext;
            NextLabel = nextLabel;
            ShowNotYet = showNotYet;
            NotYetLabel = notYetLabel;
            ShowSkip = showSkip;
            ShowLearnMore = showLearnMore;
            ShowElements = showElements;
            BlockHole = blockHole;
            Fallback = fallback;
        }

        public static TutorialOverlayModel Build(TutorialSequencer sequencer, TutorialStepData step, TutorialProgress progress,
            bool fallbackActive)
        {
            if (step == null)
            {
                return new TutorialOverlayModel(true, DefaultNextLabel, false, DefaultNotYetLabel, true, false, false, true, true);
            }

            bool info = step.Kind == TutorialStepKind.Info;
            bool showNext = info || fallbackActive;
            string nextLabel = !fallbackActive && info && CompletesOnNext(sequencer, step) ? FinishLabel : DefaultNextLabel;

            bool showNotYet = !fallbackActive && HasUsableNotYet(step, progress);
            string notYetLabel = string.IsNullOrEmpty(step.NotYetLabel) ? DefaultNotYetLabel : step.NotYetLabel;

            return new TutorialOverlayModel(
                showNext,
                nextLabel,
                showNotYet,
                notYetLabel,
                TutorialRules.CanSkip(step, progress, fallbackActive),
                step.ShowLearnMore,
                step.ShowElements,
                info || fallbackActive,
                fallbackActive);
        }

        public static TutorialOverlayModel ForTip(string gotItLabel)
        {
            string label = string.IsNullOrEmpty(gotItLabel) ? DefaultGotItLabel : gotItLabel;
            return new TutorialOverlayModel(true, label, false, DefaultNotYetLabel, false, false, false, false, false, false);
        }

        private static bool HasUsableNotYet(TutorialStepData step, TutorialProgress progress)
        {
            bool isReplay = progress != null && progress.isReplay;
            foreach (var transition in step.Transitions)
            {
                if (transition != null && transition.On == TutorialSignal.NotYet && (!transition.ReplayOnly || isReplay))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CompletesOnNext(TutorialSequencer sequencer, TutorialStepData step)
        {
            foreach (var transition in step.Transitions)
            {
                if (transition != null && transition.On == TutorialSignal.Next)
                {
                    return string.IsNullOrEmpty(transition.ToStepId);
                }
            }

            if (sequencer == null)
            {
                return false;
            }

            int index = sequencer.IndexOf(step.Id);
            return index >= 0 && index == sequencer.Steps.Count - 1;
        }
    }
}
