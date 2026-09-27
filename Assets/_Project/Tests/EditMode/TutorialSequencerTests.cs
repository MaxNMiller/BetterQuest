using NUnit.Framework;
using Spaa.Flow;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialSequencerTests
    {
        private static TutorialSequencer Seq => TutorialTestScript.Sequencer();

        private static TutorialProgress At(string id, bool isReplay = false, bool firstHabitWritten = false)
        {
            return TutorialTestScript.At(id, isReplay, firstHabitWritten);
        }

        [Test]
        public void InfoStep_Next_AdvancesToNextInList()
        {
            var progress = At("welcome");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Next)));
            Assert.AreEqual("therapy", progress.currentStepId);
        }

        [Test]
        public void ActionStep_Next_IsIgnored()
        {
            var progress = At("open-journal");

            Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Next)));
            Assert.AreEqual("open-journal", progress.currentStepId);
        }

        [Test]
        public void StateEntered_MatchingState_Advances()
        {
            var progress = At("open-journal");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Entered(GameState.HabitMenu)));
            Assert.AreEqual("add-habit", progress.currentStepId);
        }

        [Test]
        public void StateEntered_OtherState_IsIgnored()
        {
            var progress = At("open-journal");

            Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Entered(GameState.Dashboard)));
            Assert.AreEqual("open-journal", progress.currentStepId);
        }

        [Test]
        public void UnrelatedSignal_IsIgnored()
        {
            var progress = At("welcome");

            Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.AttackResolved)));
            Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.MonsterDefeated)));
            Assert.AreEqual("welcome", progress.currentStepId);
        }

        [Test]
        public void Skip_OnUnlockedStep_CompletesAsSkipped()
        {
            var progress = At("welcome");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Skip)));
            Assert.IsTrue(progress.completed);
            Assert.IsTrue(progress.skipped);
            Assert.AreEqual(string.Empty, progress.currentStepId);
        }

        [Test]
        public void Skip_OnLockedStep_FirstRun_IsIgnored()
        {
            foreach (var id in new[] { "open-journal", "add-habit", "write-name" })
            {
                var progress = At(id);

                Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Skip)), id);
                Assert.IsFalse(progress.completed, id);
                Assert.AreEqual(id, progress.currentStepId);
            }
        }

        [Test]
        public void Skip_OnLockedStep_OnReplay_Completes()
        {
            var progress = At("write-name", isReplay: true);

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Skip)));
            Assert.IsTrue(progress.completed);
        }

        [Test]
        public void Skip_OnLockedStep_DuringFallback_Completes()
        {
            var progress = At("add-habit");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Skip), fallbackActive: true));
            Assert.IsTrue(progress.completed);
        }

        [Test]
        public void EditorClosedWithoutSave_ReturnsToAddHabit()
        {
            var progress = At("write-name");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.HabitEditorClosed)));
            Assert.AreEqual("add-habit", progress.currentStepId);
            Assert.IsFalse(progress.firstHabitWritten);
        }

        [Test]
        public void HabitSaved_MarksFirstHabitAndAdvances()
        {
            var progress = At("write-name");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.HabitSaved)));
            Assert.IsTrue(progress.firstHabitWritten);
            Assert.AreEqual("habit-saved", progress.currentStepId);
        }

        [Test]
        public void HabitSaved_OnOtherStep_StillMarksFirstHabit()
        {
            var progress = At("welcome");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.HabitSaved)));
            Assert.IsTrue(progress.firstHabitWritten);
            Assert.AreEqual("welcome", progress.currentStepId);
        }

        [Test]
        public void ReplayOnlyNotYet_IgnoredOnFirstRun()
        {
            var progress = At("write-name");

            Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.NotYet)));
            Assert.AreEqual("write-name", progress.currentStepId);
        }

        [Test]
        public void ReplayOnlyNotYet_FollowedOnReplay()
        {
            var progress = At("write-name", isReplay: true);

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.NotYet)));
            Assert.AreEqual("back-to-menu", progress.currentStepId);
        }

        [Test]
        public void NotYet_OnFirstAttack_GoesToComeBack()
        {
            var progress = At("first-attack");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.NotYet)));
            Assert.AreEqual("come-back", progress.currentStepId);
        }

        [Test]
        public void ExplicitNextTransition_OverridesListOrder()
        {
            var progress = At("hit");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Next)));
            Assert.AreEqual("wrap-up", progress.currentStepId);
        }

        [Test]
        public void EmptyTarget_CompletesNotSkipped()
        {
            var progress = At("wrap-up");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Next)));
            Assert.IsTrue(progress.completed);
            Assert.IsFalse(progress.skipped);
        }

        [Test]
        public void PlayUnavailable_GoesToRestDay_ThenCompletes()
        {
            var progress = At("play");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.PlayUnavailable)));
            Assert.AreEqual("rest-day", progress.currentStepId);
            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Next)));
            Assert.IsTrue(progress.completed);
        }

        [Test]
        public void FallbackNext_OnActionStep_AdvancesInListOrder()
        {
            var progress = At("back-to-menu");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.FallbackNext), fallbackActive: true));
            Assert.AreEqual("play", progress.currentStepId);
        }

        [Test]
        public void FallbackNext_OnLastStep_Completes()
        {
            var progress = At("rest-day");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.FallbackNext), fallbackActive: true));
            Assert.IsTrue(progress.completed);
        }

        [Test]
        public void UnknownStepId_RecoversToFirstStep()
        {
            var progress = At("step-that-was-renamed");

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Next)));
            Assert.AreEqual("welcome", progress.currentStepId);
            Assert.IsFalse(progress.completed);
        }

        [Test]
        public void EmptyScript_CompletesInsteadOfLooping()
        {
            var progress = At("welcome");

            Assert.IsTrue(new TutorialSequencer(new TutorialStepData[0]).Handle(progress, TutorialEvent.Of(TutorialSignal.Next)));
            Assert.IsTrue(progress.completed);
        }

        [Test]
        public void Completed_IgnoresEverythingButReplay()
        {
            var progress = TutorialProgress.CompletedSilently(1);

            foreach (TutorialSignal signal in System.Enum.GetValues(typeof(TutorialSignal)))
            {
                if (signal == TutorialSignal.Replay)
                {
                    continue;
                }

                Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Of(signal)), signal.ToString());
            }

            Assert.IsTrue(progress.completed);
        }

        [Test]
        public void Replay_FromCompleted_RestartsAsReplay()
        {
            var progress = TutorialProgress.CompletedSilently(1);
            progress.skipped = true;

            Assert.IsTrue(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Replay)));
            Assert.IsFalse(progress.completed);
            Assert.IsFalse(progress.skipped);
            Assert.IsTrue(progress.isReplay);
            Assert.AreEqual("welcome", progress.currentStepId);
        }

        [Test]
        public void Replay_WhileRunning_IsIgnored()
        {
            var progress = At("play");

            Assert.IsFalse(Seq.Handle(progress, TutorialEvent.Of(TutorialSignal.Replay)));
            Assert.AreEqual("play", progress.currentStepId);
            Assert.IsFalse(progress.isReplay);
        }

        [Test]
        public void FirstRun_HappyPath_Completes()
        {
            var seq = Seq;
            var progress = TutorialProgress.FirstRun("welcome", 1);
            TutorialEvent[] events =
            {
                TutorialEvent.Of(TutorialSignal.Next),
                TutorialEvent.Of(TutorialSignal.Next),
                TutorialEvent.Entered(GameState.HabitMenu),
                TutorialEvent.Of(TutorialSignal.HabitEditorOpened),
                TutorialEvent.Of(TutorialSignal.HabitEditorClosed),
                TutorialEvent.Of(TutorialSignal.HabitEditorOpened),
                TutorialEvent.Of(TutorialSignal.HabitSaved),
                TutorialEvent.Of(TutorialSignal.Next),
                TutorialEvent.Entered(GameState.MainMenu),
                TutorialEvent.Entered(GameState.Battle),
                TutorialEvent.Of(TutorialSignal.Next),
                TutorialEvent.Of(TutorialSignal.AttackResolved),
                TutorialEvent.Of(TutorialSignal.Next),
                TutorialEvent.Of(TutorialSignal.Next)
            };

            foreach (var evt in events)
            {
                Assert.IsTrue(seq.Handle(progress, evt), "stalled at " + progress.currentStepId + " on " + evt.Signal);
            }

            Assert.IsTrue(progress.completed);
            Assert.IsFalse(progress.skipped);
            Assert.IsTrue(progress.firstHabitWritten);
        }

        [Test]
        public void CanSkip_LockedStep_UnlocksOnceHabitWritten()
        {
            var step = Seq.Find("add-habit");

            Assert.IsFalse(TutorialRules.CanSkip(step, At("add-habit"), false));
            Assert.IsTrue(TutorialRules.CanSkip(step, At("add-habit", firstHabitWritten: true), false));
            Assert.IsTrue(TutorialRules.CanSkip(Seq.Find("welcome"), At("welcome"), false));
            Assert.IsFalse(TutorialRules.CanSkip(step, TutorialProgress.CompletedSilently(1), true));
        }
    }
}
