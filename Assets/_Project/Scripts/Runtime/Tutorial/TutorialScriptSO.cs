using UnityEngine;

namespace Spaa.Tutorial
{
    [CreateAssetMenu(menuName = "Spaa/Tutorial/Script", fileName = "TutorialScript")]
    public class TutorialScriptSO : ScriptableObject
    {
        [Tooltip("In order. Info steps without a Next transition continue to the next entry. Menu-only terminal steps go last (see TutorialResume).")]
        [SerializeField] private TutorialStepData[] steps = new TutorialStepData[0];
        [Tooltip("Replaces the 'play' step's lines when Play is blocked by an empty pillar. {0} = the missing pillar names.")]
        [SerializeField] private string playMissingHabitsLine = "Every pillar needs a habit first. Tap Habits and add one for {0}.";
        [Tooltip("Replaces the 'play' step's lines when a level-up is waiting to be claimed.")]
        [SerializeField] private string playClaimRewardLine = "Your reward is waiting! Tap Claim Reward.";
        [Tooltip("Asked when the player taps the Main Menu's ? button.")]
        [SerializeField] private string replayPrompt = "Shall I show you around again?";
        [Tooltip("One-off callouts shown after the tutorial, each the first time its state is entered.")]
        [SerializeField] private TutorialTipData[] tips = new TutorialTipData[0];
        [SerializeField] private string tipGotItLabel = "Got it";

        public TutorialStepData[] Steps => steps;
        public string PlayMissingHabitsLine => playMissingHabitsLine;
        public string PlayClaimRewardLine => playClaimRewardLine;
        public string ReplayPrompt => replayPrompt;
        public TutorialTipData[] Tips => tips;
        public string TipGotItLabel => tipGotItLabel;

#if UNITY_EDITOR
        public void EditorSetSteps(TutorialStepData[] value)
        {
            steps = value ?? new TutorialStepData[0];
        }

        public void EditorSetTips(TutorialTipData[] value)
        {
            tips = value ?? new TutorialTipData[0];
        }
#endif
    }
}
