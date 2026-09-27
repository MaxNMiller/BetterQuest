using Spaa.Flow;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public static class TutorialTestScript
    {
        public static TutorialStepData[] Steps()
        {
            return new[]
            {
                Info("welcome", TutorialScene.Menu),
                Info("therapy", TutorialScene.Menu),
                TutorialStepData.Create("open-journal", TutorialStepKind.Action, TutorialScene.Menu,
                    new[] { new TutorialTransition(TutorialSignal.StateEntered, "add-habit", GameState.HabitMenu) },
                    lockSkipUntilFirstHabit: true, targetName: "habit-menu-button"),
                TutorialStepData.Create("add-habit", TutorialStepKind.Action, TutorialScene.Menu,
                    new[]
                    {
                        new TutorialTransition(TutorialSignal.HabitEditorOpened, "write-name"),
                        new TutorialTransition(TutorialSignal.NotYet, "back-to-menu", replayOnly: true)
                    },
                    resumeStepId: "open-journal", lockSkipUntilFirstHabit: true, targetName: "habit-add-button"),
                TutorialStepData.Create("write-name", TutorialStepKind.Action, TutorialScene.Menu,
                    new[]
                    {
                        new TutorialTransition(TutorialSignal.HabitSaved, "habit-saved"),
                        new TutorialTransition(TutorialSignal.HabitEditorClosed, "add-habit"),
                        new TutorialTransition(TutorialSignal.NotYet, "back-to-menu", replayOnly: true)
                    },
                    resumeStepId: "open-journal", lockSkipUntilFirstHabit: true, targetName: "habit-name-field"),
                Info("habit-saved", TutorialScene.Menu),
                TutorialStepData.Create("back-to-menu", TutorialStepKind.Action, TutorialScene.Menu,
                    new[] { new TutorialTransition(TutorialSignal.StateEntered, "play", GameState.MainMenu) },
                    targetName: "habit-menu-back-button"),
                TutorialStepData.Create("play", TutorialStepKind.Action, TutorialScene.Menu,
                    new[]
                    {
                        new TutorialTransition(TutorialSignal.StateEntered, "battle-intro", GameState.Battle),
                        new TutorialTransition(TutorialSignal.PlayUnavailable, "rest-day")
                    },
                    targetName: "play-button"),
                Info("battle-intro", TutorialScene.Battle),
                TutorialStepData.Create("first-attack", TutorialStepKind.Action, TutorialScene.Battle,
                    new[]
                    {
                        new TutorialTransition(TutorialSignal.AttackResolved, "hit"),
                        new TutorialTransition(TutorialSignal.NotYet, "come-back")
                    },
                    targetName: "attack-sheet"),
                TutorialStepData.Create("hit", TutorialStepKind.Info, TutorialScene.Battle,
                    new[] { new TutorialTransition(TutorialSignal.Next, "wrap-up") }),
                Info("come-back", TutorialScene.Battle),
                TutorialStepData.Create("wrap-up", TutorialStepKind.Info, TutorialScene.Battle,
                    new[] { new TutorialTransition(TutorialSignal.Next, string.Empty) }),
                TutorialStepData.Create("rest-day", TutorialStepKind.Info, TutorialScene.Menu,
                    new[] { new TutorialTransition(TutorialSignal.Next, string.Empty) })
            };
        }

        public static TutorialSequencer Sequencer()
        {
            return new TutorialSequencer(Steps());
        }

        public static TutorialProgress At(string stepId, bool isReplay = false, bool firstHabitWritten = false)
        {
            var progress = TutorialProgress.FirstRun(stepId, 1);
            progress.isReplay = isReplay;
            progress.firstHabitWritten = firstHabitWritten;
            return progress;
        }

        private static TutorialStepData Info(string id, TutorialScene scene)
        {
            return TutorialStepData.Create(id, TutorialStepKind.Info, scene);
        }
    }
}
