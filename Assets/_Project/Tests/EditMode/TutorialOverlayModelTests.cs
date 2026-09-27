using NUnit.Framework;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialOverlayModelTests
    {
        private static TutorialOverlayModel Build(string stepId, bool isReplay = false, bool firstHabitWritten = false,
            bool fallback = false)
        {
            var sequencer = TutorialTestScript.Sequencer();
            return TutorialOverlayModel.Build(sequencer, sequencer.Find(stepId),
                TutorialTestScript.At(stepId, isReplay, firstHabitWritten), fallback);
        }

        [Test]
        public void InfoStep_ShowsNext_AndBlocksTheHole()
        {
            var model = Build("welcome");

            Assert.IsTrue(model.ShowNext);
            Assert.AreEqual(TutorialOverlayModel.DefaultNextLabel, model.NextLabel);
            Assert.IsTrue(model.BlockHole);
            Assert.IsTrue(model.ShowSkip);
            Assert.IsFalse(model.ShowNotYet);
        }

        [Test]
        public void ActionStep_HidesNext_AndLetsTapsThrough()
        {
            var model = Build("play");

            Assert.IsFalse(model.ShowNext);
            Assert.IsFalse(model.BlockHole);
            Assert.IsTrue(model.ShowSkip);
        }

        [Test]
        public void FinalInfoStep_SaysLetsGo()
        {
            Assert.AreEqual(TutorialOverlayModel.FinishLabel, Build("wrap-up").NextLabel);
            Assert.AreEqual(TutorialOverlayModel.FinishLabel, Build("rest-day").NextLabel);
            Assert.AreEqual(TutorialOverlayModel.DefaultNextLabel, Build("hit").NextLabel);
        }

        [Test]
        public void RequiredHabitStep_FirstRun_HasNoSkipAndNoNotYet()
        {
            var model = Build("write-name");

            Assert.IsFalse(model.ShowSkip);
            Assert.IsFalse(model.ShowNotYet);
            Assert.IsFalse(model.ShowNext);
        }

        [Test]
        public void RequiredHabitStep_OnReplay_OffersSkipAndNotYet()
        {
            var model = Build("write-name", isReplay: true);

            Assert.IsTrue(model.ShowSkip);
            Assert.IsTrue(model.ShowNotYet);
        }

        [Test]
        public void FirstAttack_OffersNotYetWithDefaultLabel()
        {
            var model = Build("first-attack");

            Assert.IsTrue(model.ShowNotYet);
            Assert.AreEqual(TutorialOverlayModel.DefaultNotYetLabel, model.NotYetLabel);
        }

        [Test]
        public void CustomNotYetLabel_IsUsed()
        {
            var step = TutorialStepData.Create("x", TutorialStepKind.Action, TutorialScene.Menu,
                new[] { new TutorialTransition(TutorialSignal.NotYet, "y") }).WithNotYetLabel("Skip for now");
            var model = TutorialOverlayModel.Build(null, step, TutorialTestScript.At("x"), false);

            Assert.AreEqual("Skip for now", model.NotYetLabel);
        }

        [Test]
        public void Fallback_ShowsNextAndSkip_BlocksHole_HidesNotYet()
        {
            var model = Build("write-name", fallback: true);

            Assert.IsTrue(model.Fallback);
            Assert.IsTrue(model.ShowNext);
            Assert.AreEqual(TutorialOverlayModel.DefaultNextLabel, model.NextLabel);
            Assert.IsTrue(model.ShowSkip);
            Assert.IsTrue(model.BlockHole);
            Assert.IsFalse(model.ShowNotYet);
        }

        [Test]
        public void LearnMoreAndElements_FollowTheStep()
        {
            var step = TutorialStepData.Create("x", TutorialStepKind.Info, TutorialScene.Menu).WithLearnMore().WithElements();
            var model = TutorialOverlayModel.Build(null, step, TutorialTestScript.At("x"), false);

            Assert.IsTrue(model.ShowLearnMore);
            Assert.IsTrue(model.ShowElements);
            Assert.IsFalse(Build("welcome").ShowLearnMore);
        }

        [Test]
        public void NullStep_IsASafeFallback()
        {
            var model = TutorialOverlayModel.Build(null, null, null, false);

            Assert.IsTrue(model.Fallback);
            Assert.IsTrue(model.ShowNext);
            Assert.IsTrue(model.ShowSkip);
        }

        [Test]
        public void Tip_NeitherDimsNorBlocks_AndOnlyOffersGotIt()
        {
            var model = TutorialOverlayModel.ForTip(null);

            Assert.IsFalse(model.Dim);
            Assert.IsFalse(model.BlockHole);
            Assert.IsTrue(model.ShowNext);
            Assert.AreEqual(TutorialOverlayModel.DefaultGotItLabel, model.NextLabel);
            Assert.IsFalse(model.ShowSkip);
            Assert.IsFalse(model.ShowNotYet);
            Assert.IsTrue(Build("welcome").Dim, "Tutorial steps always dim.");
        }
    }
}
