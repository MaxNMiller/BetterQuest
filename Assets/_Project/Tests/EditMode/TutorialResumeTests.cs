using NUnit.Framework;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialResumeTests
    {
        private static string Resolve(string stepId, TutorialScene scene)
        {
            var step = TutorialResume.ResolveStart(TutorialTestScript.Sequencer(), TutorialTestScript.At(stepId), scene);
            return step == null ? null : step.Id;
        }

        [Test]
        public void SameScene_NoResumePoint_ShowsCurrentStep()
        {
            Assert.AreEqual("therapy", Resolve("therapy", TutorialScene.Menu));
            Assert.AreEqual("play", Resolve("play", TutorialScene.Menu));
        }

        [Test]
        public void HabitEditorSteps_ReloadedInMenu_ResumeAtOpenJournal()
        {
            Assert.AreEqual("open-journal", Resolve("write-name", TutorialScene.Menu));
            Assert.AreEqual("open-journal", Resolve("add-habit", TutorialScene.Menu));
        }

        [Test]
        public void BattleStep_LoadedInBattle_ShowsItself()
        {
            Assert.AreEqual("battle-intro", Resolve("battle-intro", TutorialScene.Battle));
            Assert.AreEqual("first-attack", Resolve("first-attack", TutorialScene.Battle));
        }

        [Test]
        public void BattleStep_LoadedInMenu_WalksBackToPlay()
        {
            Assert.AreEqual("play", Resolve("battle-intro", TutorialScene.Menu));
            Assert.AreEqual("play", Resolve("wrap-up", TutorialScene.Menu));
        }

        [Test]
        public void MenuStep_LoadedInBattle_ShowsNothing()
        {
            Assert.IsNull(Resolve("welcome", TutorialScene.Battle));
            Assert.IsNull(Resolve("play", TutorialScene.Battle));
        }

        [Test]
        public void UnknownStep_StartsOverOnlyInTheFirstStepsScene()
        {
            Assert.AreEqual("welcome", Resolve("renamed-step", TutorialScene.Menu));
            Assert.IsNull(Resolve("renamed-step", TutorialScene.Battle));
        }

        [Test]
        public void CompletedOrMissingProgress_ShowsNothing()
        {
            var seq = TutorialTestScript.Sequencer();

            Assert.IsNull(TutorialResume.ResolveStart(seq, TutorialProgress.CompletedSilently(1), TutorialScene.Menu));
            Assert.IsNull(TutorialResume.ResolveStart(seq, null, TutorialScene.Menu));
            Assert.IsNull(TutorialResume.ResolveStart(new TutorialSequencer(null), TutorialTestScript.At("welcome"), TutorialScene.Menu));
        }
    }
}
