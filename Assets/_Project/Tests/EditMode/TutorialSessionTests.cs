using System.Collections.Generic;
using NUnit.Framework;
using Spaa.Flow;
using Spaa.Save;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialSessionTests
    {
        private class MemoryStore : ITutorialProgressStore
        {
            public TutorialProgress Stored;
            public int SaveCount;

            public TutorialProgress Load()
            {
                return Stored == null ? null : UnityEngine.JsonUtility.FromJson<TutorialProgress>(UnityEngine.JsonUtility.ToJson(Stored));
            }

            public void Save(TutorialProgress progress)
            {
                SaveCount++;
                Stored = UnityEngine.JsonUtility.FromJson<TutorialProgress>(UnityEngine.JsonUtility.ToJson(progress));
            }
        }

        private MemoryStore store;

        [SetUp]
        public void SetUp()
        {
            store = new MemoryStore();
        }

        private TutorialSession Session(TutorialScene scene = TutorialScene.Menu, bool enabled = true)
        {
            return new TutorialSession(TutorialTestScript.Sequencer(), store, scene, enabled, 1, false);
        }

        private static SaveData ExistingPlayer()
        {
            return new SaveData { currentDayKey = "2026-09-01" };
        }

        private TutorialSession MenuSessionAt(string stepId, bool isReplay = false, bool firstHabitWritten = false)
        {
            store.Stored = TutorialTestScript.At(stepId, isReplay, firstHabitWritten);
            var session = Session();
            session.Begin(new SaveData(), GameState.MainMenu);
            return session;
        }

        [Test]
        public void FreshSave_OnMainMenu_StartsAtWelcomeAndPersists()
        {
            var session = Session();

            Assert.AreEqual(TutorialStartDecision.Start, session.Begin(new SaveData(), GameState.MainMenu));
            Assert.AreEqual("welcome", session.VisibleStep.Id);
            Assert.AreEqual("welcome", store.Stored.currentStepId);
            Assert.IsFalse(store.Stored.completed);
        }

        [Test]
        public void ExistingPlayer_IsMarkedCompleteWithoutShowingAnything()
        {
            var session = Session();

            Assert.AreEqual(TutorialStartDecision.MarkCompleteSilently, session.Begin(ExistingPlayer(), GameState.MainMenu));
            Assert.IsNull(session.VisibleStep);
            Assert.IsTrue(store.Stored.completed);
        }

        [Test]
        public void Disabled_ShowsNothingAndWritesNothing()
        {
            var session = Session(enabled: false);

            Assert.AreEqual(TutorialStartDecision.Disabled, session.Begin(new SaveData(), GameState.MainMenu));
            Assert.IsFalse(session.Replay());
            Assert.IsNull(session.VisibleStep);
            Assert.AreEqual(0, store.SaveCount);
        }

        [Test]
        public void FreshSave_BootingIntoBattle_WaitsForTheMenu()
        {
            var session = Session(TutorialScene.Battle);

            session.Begin(new SaveData(), GameState.Battle);

            Assert.IsNull(session.VisibleStep);
            Assert.AreEqual(0, store.SaveCount);
        }

        [Test]
        public void EnteringBattle_PersistsTheBattleStepBeforeTheSceneLoads()
        {
            var session = MenuSessionAt("play", firstHabitWritten: true);

            session.StateEntered(GameState.Battle);

            Assert.AreEqual("battle-intro", store.Stored.currentStepId);
            Assert.IsNull(session.VisibleStep, "A Battle step never draws in the Menu.");

            var battle = Session(TutorialScene.Battle);
            battle.Begin(new SaveData(), GameState.Battle);
            Assert.AreEqual("battle-intro", battle.VisibleStep.Id);
        }

        [Test]
        public void QuitDuringWriteName_ResumesAtOpenJournal()
        {
            var session = MenuSessionAt("write-name");

            Assert.AreEqual("open-journal", session.VisibleStep.Id);
            Assert.AreEqual("open-journal", store.Stored.currentStepId);
        }

        [Test]
        public void BattleStepAfterRestart_ResumesAtPlayInTheMenu()
        {
            var session = MenuSessionAt("first-attack", firstHabitWritten: true);

            Assert.AreEqual("play", session.VisibleStep.Id);
        }

        [Test]
        public void LeavingTheStepsScreen_HidesItUntilThePlayerReturns()
        {
            var session = MenuSessionAt("play", firstHabitWritten: true);

            session.StateEntered(GameState.HabitMenu);
            Assert.IsNull(session.VisibleStep);
            Assert.AreEqual("play", store.Stored.currentStepId);

            session.StateEntered(GameState.MainMenu);
            Assert.AreEqual("play", session.VisibleStep.Id);
        }

        [Test]
        public void LevelUpOverTheBattle_HidesTheBattleStep()
        {
            store.Stored = TutorialTestScript.At("first-attack", firstHabitWritten: true);
            var session = Session(TutorialScene.Battle);
            session.Begin(new SaveData(), GameState.Battle);

            session.StateEntered(GameState.LevelUp);

            Assert.IsNull(session.VisibleStep);
        }

        [Test]
        public void SoftReminderBeforeTheBattleBegins_KeepsTheStepHidden()
        {
            store.Stored = TutorialTestScript.At("battle-intro", firstHabitWritten: true);
            var session = Session(TutorialScene.Battle);
            session.StateEntered(GameState.SoftReminder);

            session.Begin(new SaveData(), session.State);

            Assert.IsNull(session.VisibleStep);
        }

        [Test]
        public void ActionSteps_AdvanceOnTheirSignals()
        {
            var session = MenuSessionAt("open-journal");

            Assert.IsTrue(session.StateEntered(GameState.HabitMenu));
            Assert.AreEqual("add-habit", session.VisibleStep.Id);
            Assert.IsTrue(session.Signal(TutorialSignal.HabitEditorOpened));
            Assert.AreEqual("write-name", session.VisibleStep.Id);
            Assert.IsTrue(session.Signal(TutorialSignal.HabitSaved));
            Assert.AreEqual("habit-saved", session.VisibleStep.Id);
            Assert.IsTrue(store.Stored.firstHabitWritten);
        }

        [Test]
        public void CancelledEditor_ReturnsToAddHabit_WithoutSkip()
        {
            var session = MenuSessionAt("open-journal");
            session.StateEntered(GameState.HabitMenu);
            session.Signal(TutorialSignal.HabitEditorOpened);

            session.Signal(TutorialSignal.HabitEditorClosed);

            Assert.AreEqual("add-habit", session.VisibleStep.Id);
            Assert.IsFalse(session.Signal(TutorialSignal.Skip));
            Assert.IsTrue(session.IsActive);
        }

        [Test]
        public void Fallback_AllowsSkipAndNextMovesOn()
        {
            var session = MenuSessionAt("open-journal");

            session.EnterFallback();
            Assert.IsTrue(session.Fallback);
            Assert.IsTrue(session.Signal(TutorialSignal.Next));
            Assert.AreEqual("add-habit", store.Stored.currentStepId);
            Assert.IsFalse(session.Fallback, "A new step starts without the fallback.");

            session.EnterFallback();
            Assert.IsTrue(session.Signal(TutorialSignal.Skip));
            Assert.IsTrue(store.Stored.completed);
            Assert.IsTrue(store.Stored.skipped);
        }

        [Test]
        public void DefeatedToday_PlayMovesToTheRestDayEnding()
        {
            var session = MenuSessionAt("play", firstHabitWritten: true);

            Assert.IsFalse(session.ApplyPlayVariant(TutorialPlayVariant.Ready));
            Assert.IsFalse(session.ApplyPlayVariant(TutorialPlayVariant.MissingHabits));
            Assert.IsTrue(session.ApplyPlayVariant(TutorialPlayVariant.Defeated));
            Assert.AreEqual("rest-day", session.VisibleStep.Id);

            Assert.IsTrue(session.Signal(TutorialSignal.Next));
            Assert.IsTrue(store.Stored.completed);
        }

        [Test]
        public void Replay_RestartsACompletedTutorialOnlyFromTheMenu()
        {
            store.Stored = TutorialProgress.CompletedSilently(1);
            var battle = Session(TutorialScene.Battle);
            battle.Begin(new SaveData(), GameState.Battle);
            Assert.IsFalse(battle.Replay());

            var session = Session();
            session.Begin(ExistingPlayer(), GameState.MainMenu);
            Assert.IsTrue(session.Replay());
            Assert.AreEqual("welcome", session.VisibleStep.Id);
            Assert.IsTrue(store.Stored.isReplay);
            Assert.IsFalse(session.Replay(), "No second replay over a running one.");
        }

        [Test]
        public void Replay_OffersSkipForNowOnTheHabitSteps()
        {
            var session = MenuSessionAt("add-habit", isReplay: true);
            session.StateEntered(GameState.HabitMenu);
            Assert.AreEqual("add-habit", session.VisibleStep != null ? session.VisibleStep.Id : null, "precondition");

            Assert.IsTrue(session.Signal(TutorialSignal.NotYet));

            Assert.AreEqual("back-to-menu", store.Stored.currentStepId);
        }

        [Test]
        public void DebugReset_FileWithFirstRun_RestartsEvenWithHistory()
        {
            store.Stored = TutorialProgress.FirstRun("welcome", 1);
            var session = Session();

            Assert.AreEqual(TutorialStartDecision.Resume, session.Begin(ExistingPlayer(), GameState.MainMenu));
            Assert.AreEqual("welcome", session.VisibleStep.Id);
        }

        [Test]
        public void Tips_WaitForTheTutorialAndShowOnce()
        {
            var tips = new List<TutorialTipData>
            {
                TutorialTipData.Create("tip-progress", GameState.MainMenu, TutorialScene.Menu, "dashboard-button", afterFirstBattle: true)
            };

            var active = MenuSessionAt("welcome");
            Assert.IsNull(active.TakeTip(tips, GameState.MainMenu, true), "Never during the tutorial.");

            store.Stored = TutorialProgress.CompletedSilently(1);
            var session = Session();
            session.Begin(ExistingPlayer(), GameState.MainMenu);
            Assert.IsNull(session.TakeTip(tips, GameState.MainMenu, false), "Needs a battle first.");
            Assert.AreEqual("tip-progress", session.TakeTip(tips, GameState.MainMenu, true).Id);
            CollectionAssert.Contains(store.Stored.seenTips, "tip-progress");
            Assert.IsNull(session.TakeTip(tips, GameState.MainMenu, true));
        }
    }
}
