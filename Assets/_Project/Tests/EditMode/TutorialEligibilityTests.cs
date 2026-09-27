using NUnit.Framework;
using Spaa.Save;
using Spaa.Tutorial;

namespace Spaa.Tests
{
    public class TutorialEligibilityTests
    {
        private static TutorialStartDecision Evaluate(TutorialProgress progress, SaveData save, bool enabled = true,
            int version = 1, bool replayOnBump = false)
        {
            return TutorialEligibility.Evaluate(progress, save, enabled, version, replayOnBump);
        }

        [Test]
        public void Disabled_WinsOverEverything()
        {
            Assert.AreEqual(TutorialStartDecision.Disabled, Evaluate(null, new SaveData(), enabled: false));
            Assert.AreEqual(TutorialStartDecision.Disabled, Evaluate(TutorialTestScript.At("play"), new SaveData(), enabled: false));
        }

        [Test]
        public void NoFile_FreshSave_Starts()
        {
            Assert.AreEqual(TutorialStartDecision.Start, Evaluate(null, new SaveData()));
            Assert.AreEqual(TutorialStartDecision.Start, Evaluate(null, null));
        }

        [Test]
        public void NoFile_SeededHabitsOnly_StillStarts()
        {
            var save = new SaveData { habitsSeeded = true };

            Assert.AreEqual(TutorialStartDecision.Start, Evaluate(null, save));
        }

        [Test]
        public void NoFile_ExistingPlayerWithHistory_IsMarkedCompleteSilently()
        {
            var save = new SaveData();
            save.history.Add(new DayRecord { dayKey = "2026-09-26" });

            Assert.AreEqual(TutorialStartDecision.MarkCompleteSilently, Evaluate(null, save));
        }

        [Test]
        public void NoFile_ExistingPlayerWithDayKey_IsMarkedCompleteSilently()
        {
            var save = new SaveData { currentDayKey = "2026-09-26" };

            Assert.AreEqual(TutorialStartDecision.MarkCompleteSilently, Evaluate(null, save));
        }

        [Test]
        public void UnfinishedFile_Resumes()
        {
            var save = new SaveData { currentDayKey = "2026-09-26" };

            Assert.AreEqual(TutorialStartDecision.Resume, Evaluate(TutorialTestScript.At("welcome"), save));
        }

        [Test]
        public void CompletedFile_DoesNothing()
        {
            Assert.AreEqual(TutorialStartDecision.None, Evaluate(TutorialProgress.CompletedSilently(1), new SaveData()));
        }

        [Test]
        public void CompletedOlderVersion_ReplaysOnlyWhenFlagged()
        {
            var old = TutorialProgress.CompletedSilently(1);

            Assert.AreEqual(TutorialStartDecision.None, Evaluate(old, new SaveData(), version: 2));
            Assert.AreEqual(TutorialStartDecision.Replay, Evaluate(old, new SaveData(), version: 2, replayOnBump: true));
            Assert.AreEqual(TutorialStartDecision.None, Evaluate(old, new SaveData(), version: 1, replayOnBump: true));
        }
    }
}
