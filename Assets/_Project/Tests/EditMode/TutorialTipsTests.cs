using NUnit.Framework;
using Spaa.Flow;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialTipsTests
    {
        private static readonly TutorialTipData[] Tips =
        {
            TutorialTipData.Create("tip-levelup", GameState.LevelUp, TutorialScene.Battle, "level-up-featured"),
            TutorialTipData.Create("tip-reminder", GameState.SoftReminder, TutorialScene.Battle, "adjust-habits-button"),
            TutorialTipData.Create("tip-progress", GameState.MainMenu, TutorialScene.Menu, "dashboard-button", afterFirstBattle: true)
        };

        private static TutorialProgress Completed()
        {
            return TutorialProgress.CompletedSilently(1);
        }

        [Test]
        public void EachTip_MatchesItsStateAndScene()
        {
            var progress = Completed();

            Assert.AreEqual("tip-levelup", TutorialTips.Pick(Tips, progress, GameState.LevelUp, TutorialScene.Battle, true).Id);
            Assert.AreEqual("tip-reminder", TutorialTips.Pick(Tips, progress, GameState.SoftReminder, TutorialScene.Battle, true).Id);
            Assert.AreEqual("tip-progress", TutorialTips.Pick(Tips, progress, GameState.MainMenu, TutorialScene.Menu, true).Id);
            Assert.IsNull(TutorialTips.Pick(Tips, progress, GameState.MainMenu, TutorialScene.Battle, true));
            Assert.IsNull(TutorialTips.Pick(Tips, progress, GameState.HabitMenu, TutorialScene.Menu, true));
        }

        [Test]
        public void SeenTip_IsNotShownAgain()
        {
            var progress = Completed();

            Assert.IsTrue(TutorialTips.MarkSeen(progress, "tip-levelup"));
            Assert.IsFalse(TutorialTips.MarkSeen(progress, "tip-levelup"));
            Assert.IsNull(TutorialTips.Pick(Tips, progress, GameState.LevelUp, TutorialScene.Battle, true));
        }

        [Test]
        public void NoTips_WhileTheTutorialOrAReplayRuns()
        {
            Assert.IsNull(TutorialTips.Pick(Tips, TutorialTestScript.At("welcome"), GameState.LevelUp, TutorialScene.Battle, true));
            Assert.IsNull(TutorialTips.Pick(Tips, TutorialTestScript.At("first-attack", isReplay: true), GameState.LevelUp,
                TutorialScene.Battle, true));
            Assert.IsNull(TutorialTips.Pick(Tips, null, GameState.LevelUp, TutorialScene.Battle, true));
        }

        [Test]
        public void AfterFirstBattleTip_WaitsForABattle()
        {
            Assert.IsNull(TutorialTips.Pick(Tips, Completed(), GameState.MainMenu, TutorialScene.Menu, false));
        }

        [Test]
        public void SeenTips_SurviveAStoreRoundTrip()
        {
            string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "spaa-tips-" + System.Guid.NewGuid().ToString("N"));
            var store = new TutorialProgressStore(System.IO.Path.Combine(dir, "tutorial.json"));
            try
            {
                var progress = Completed();
                TutorialTips.MarkSeen(progress, "tip-reminder");
                store.Save(progress);

                CollectionAssert.AreEqual(new[] { "tip-reminder" }, store.Load().seenTips);
            }
            finally
            {
                if (System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.Delete(dir, true);
                }
            }
        }
    }
}
